#!/usr/bin/env bash

set -euo pipefail

readonly MONITOR_NAME="Oficina Mecanica API - AWS - Health"
readonly CONDITION_NAMES=(
  "Falhas no processamento de ordens de servico - AWS"
  "Indisponibilidade do healthcheck - AWS"
)

require_command() {
  local command_name="$1"

  if ! command -v "${command_name}" >/dev/null 2>&1; then
    echo "Comando obrigatorio nao encontrado: ${command_name}" >&2
    exit 1
  fi
}

require_command curl
require_command jq

account_id="${NEW_RELIC_ACCOUNT_ID:-}"

if [[ -z "${account_id}" ]]; then
  read -r -p "New Relic Account ID: " account_id
fi

if [[ ! "${account_id}" =~ ^[0-9]+$ ]]; then
  echo "O Account ID deve conter somente numeros." >&2
  exit 1
fi

api_key="${NEW_RELIC_API_KEY:-}"

if [[ -z "${api_key}" ]]; then
  read -r -s -p "New Relic User API Key: " api_key
  printf '\n'
fi

if [[ -z "${api_key}" ]]; then
  echo "A User API Key nao pode ficar vazia." >&2
  exit 1
fi

case "${NEW_RELIC_REGION:-US}" in
  US | us)
    nerdgraph_url="https://api.newrelic.com/graphql"
    ;;
  EU | eu)
    nerdgraph_url="https://api.eu.newrelic.com/graphql"
    ;;
  *)
    echo "NEW_RELIC_REGION deve ser US ou EU." >&2
    exit 1
    ;;
esac

nerdgraph() {
  local query="$1"
  local variables="${2-}"
  local payload
  local response

  if [[ -z "${variables}" ]]; then
    variables='{}'
  fi

  if ! payload="$(
    jq -cn \
      --arg query "${query}" \
      --argjson variables "${variables}" \
      '{query: $query, variables: $variables}'
  )"; then
    echo "Falha ao montar o payload JSON para o New Relic." >&2
    return 1
  fi

  if ! response="$(
    curl --fail --silent --show-error \
      --request POST \
      --header "API-Key: ${api_key}" \
      --header "Content-Type: application/json" \
      --data-binary "${payload}" \
      "${nerdgraph_url}"
  )"; then
    echo "Falha ao chamar a API NerdGraph do New Relic." >&2
    return 1
  fi

  if ! jq -e . >/dev/null 2>&1 <<< "${response}"; then
    echo "O New Relic retornou uma resposta que nao e JSON." >&2
    return 1
  fi

  if jq -e '(.errors // []) | length > 0' >/dev/null <<< "${response}"; then
    echo "A API NerdGraph retornou erros:" >&2
    jq -r '.errors[] | "- \(.message)"' <<< "${response}" >&2
    return 1
  fi

  printf '%s\n' "${response}"
}

monitor_query="query FindAwsMonitor {
  actor {
    entitySearch(query: \"domain = 'SYNTH' AND type = 'MONITOR'\") {
      results {
        entities {
          ... on SyntheticMonitorEntityOutline {
            accountId
            guid
            name
          }
        }
      }
    }
  }
}"

monitor_response="$(nerdgraph "${monitor_query}")"
monitors="$(
  jq -c \
    --arg name "${MONITOR_NAME}" \
    --argjson account_id "${account_id}" \
    '[
      (.data.actor.entitySearch.results.entities // [])[]
      | select(.accountId == $account_id and .name == $name)
    ]' <<< "${monitor_response}"
)"

condition_query='query FindAwsConditions($accountId: Int!, $name: String!) {
  actor {
    account(id: $accountId) {
      alerts {
        nrqlConditionsSearch(searchCriteria: {name: $name}) {
          nrqlConditions {
            id
            name
            policyId
          }
        }
      }
    }
  }
}'

conditions='[]'

for condition_name in "${CONDITION_NAMES[@]}"; do
  condition_variables="$(
    jq -cn \
      --argjson account_id "${account_id}" \
      --arg name "${condition_name}" \
      '{accountId: $account_id, name: $name}'
  )"
  condition_response="$(nerdgraph "${condition_query}" "${condition_variables}")"
  condition_matches="$(
    jq -c \
      --arg name "${condition_name}" \
      '[
        (.data.actor.account.alerts.nrqlConditionsSearch.nrqlConditions // [])[]
        | select(.name == $name)
      ]' <<< "${condition_response}"
  )"
  conditions="$(
    jq -cn \
      --argjson existing "${conditions}" \
      --argjson matches "${condition_matches}" \
      '$existing + $matches | unique_by(.id)'
  )"
done

monitor_count="$(jq 'length' <<< "${monitors}")"
condition_count="$(jq 'length' <<< "${conditions}")"
total_count="$((monitor_count + condition_count))"

if [[ "${total_count}" -eq 0 ]]; then
  echo "Nenhum recurso AWS do New Relic foi encontrado. Nada para remover."
  exit 0
fi

echo "Recursos encontrados na conta ${account_id}:"
jq -r '.[] | "- Monitor: \(.name) [\(.guid)]"' <<< "${monitors}"
jq -r '.[] | "- Condicao: \(.name) [\(.id)]"' <<< "${conditions}"

printf '\nA policy e o workflow de e-mail nao serao removidos.\n'
read -r -p "Digite DESTRUIR para confirmar: " confirmation

if [[ "${confirmation}" != "DESTRUIR" ]]; then
  echo "Limpeza cancelada."
  exit 0
fi

delete_monitor_query='mutation DeleteMonitor($guid: EntityGuid!) {
  syntheticsDeleteMonitor(guid: $guid) {
    deletedGuid
  }
}'

while IFS=$'\t' read -r monitor_guid monitor_name; do
  [[ -n "${monitor_guid}" ]] || continue

  monitor_variables="$(jq -cn --arg guid "${monitor_guid}" '{guid: $guid}')"
  delete_response="$(
    nerdgraph "${delete_monitor_query}" "${monitor_variables}"
  )"
  deleted_guid="$(
    jq -r '.data.syntheticsDeleteMonitor.deletedGuid // empty' \
      <<< "${delete_response}"
  )"

  if [[ "${deleted_guid}" != "${monitor_guid}" ]]; then
    echo "O New Relic nao confirmou a remocao do monitor ${monitor_name}." >&2
    exit 1
  fi

  echo "Monitor removido: ${monitor_name}"
done < <(jq -r '.[] | [.guid, .name] | @tsv' <<< "${monitors}")

while IFS=$'\t' read -r condition_id condition_name; do
  [[ -n "${condition_id}" ]] || continue

  if [[ ! "${condition_id}" =~ ^[0-9]+$ ]]; then
    echo "ID de condicao inesperado: ${condition_id}" >&2
    exit 1
  fi

  delete_condition_query="mutation DeleteCondition {
    alertsConditionDelete(accountId: ${account_id}, id: ${condition_id}) {
      id
    }
  }"
  delete_response="$(nerdgraph "${delete_condition_query}")"
  deleted_id="$(
    jq -r '.data.alertsConditionDelete.id // empty' <<< "${delete_response}"
  )"

  if [[ "${deleted_id}" != "${condition_id}" ]]; then
    echo "O New Relic nao confirmou a remocao da condicao ${condition_name}." >&2
    exit 1
  fi

  echo "Condicao removida: ${condition_name}"
done < <(jq -r '.[] | [.id, .name] | @tsv' <<< "${conditions}")

echo "Limpeza dos recursos AWS no New Relic concluida."

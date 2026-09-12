#!/usr/bin/env bash

set -euo pipefail

PROJECT_NAME="${PROJECT_NAME:-oficina-mecanica}"

account_id="$(aws sts get-caller-identity --query Account --output text)"
state_bucket="${PROJECT_NAME}-terraform-state-${account_id}"

if ! aws s3api head-bucket --bucket "${state_bucket}" >/dev/null 2>&1; then
  echo "O bucket ${state_bucket} nao existe. Nada para destruir no bootstrap."
  exit 0
fi

echo "Esvaziando o bucket de estado ${state_bucket}."
aws s3 rm "s3://${state_bucket}" --recursive

while true; do
  versions="$(aws s3api list-object-versions --bucket "${state_bucket}" --max-keys 1000)"
  delete_payload="$(jq -c '{Objects: (((.Versions // []) + (.DeleteMarkers // [])) | map({Key: .Key, VersionId: .VersionId})), Quiet: true}' <<< "${versions}")"
  object_count="$(jq '.Objects | length' <<< "${delete_payload}")"

  if [[ "${object_count}" == "0" ]]; then
    break
  fi

  aws s3api delete-objects \
    --bucket "${state_bucket}" \
    --delete "${delete_payload}" >/dev/null
done

aws s3api delete-bucket --bucket "${state_bucket}"
echo "Bucket de estado removido: ${state_bucket}"

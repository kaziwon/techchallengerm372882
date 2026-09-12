locals {
  synthetic_healthcheck_name = "Oficina Mecanica API - AWS - Health"
  operations_dashboard_name  = "Oficina Mecanica - Operacao AWS"
}

data "newrelic_alert_policy" "monitoring" {
  name = var.new_relic_alert_policy_name
}

resource "newrelic_synthetics_monitor" "api_health" {
  status           = "ENABLED"
  name             = local.synthetic_healthcheck_name
  period           = "EVERY_MINUTE"
  uri              = "${data.terraform_remote_state.kubernetes_addons.outputs.api_gateway_url}/health"
  type             = "SIMPLE"
  locations_public = [var.new_relic_synthetics_location]

  bypass_head_request       = true
  treat_redirect_as_failure = true
  validation_string         = "Healthy"
  verify_ssl                = false

  tag {
    key    = "environment"
    values = ["aws"]
  }

  tag {
    key    = "application"
    values = [local.app_name]
  }

  depends_on = [
    kubernetes_deployment_v1.api,
    kubernetes_ingress_v1.api_public,
  ]
}

resource "newrelic_nrql_alert_condition" "service_order_failures" {
  policy_id                    = data.newrelic_alert_policy.monitoring.id
  type                         = "static"
  name                         = "Falhas no processamento de ordens de servico - AWS"
  description                  = "Abre um incidente quando uma rota de ordem de servico retorna HTTP 5xx na AWS."
  enabled                      = true
  violation_time_limit_seconds = 259200

  aggregation_window = 60
  aggregation_method = "event_timer"
  aggregation_timer  = 60
  fill_option        = "static"
  fill_value         = 0

  nrql {
    query = "FROM Transaction SELECT filter(count(*), WHERE http.statusCode >= 500) WHERE appName = '${var.new_relic_app_name}' AND name LIKE 'WebTransaction/MVC/OrdensServico/%'"
  }

  critical {
    operator              = "above"
    threshold             = 0
    threshold_duration    = 60
    threshold_occurrences = "at_least_once"
  }
}

resource "newrelic_nrql_alert_condition" "healthcheck_failures" {
  policy_id                    = data.newrelic_alert_policy.monitoring.id
  type                         = "static"
  name                         = "Indisponibilidade do healthcheck - AWS"
  description                  = "Abre um incidente quando o monitor externo nao consegue validar o endpoint /health."
  enabled                      = true
  violation_time_limit_seconds = 259200

  aggregation_window = 60
  aggregation_method = "event_timer"
  aggregation_timer  = 60
  fill_option        = "static"
  fill_value         = 0

  nrql {
    query = "FROM SyntheticCheck SELECT filter(count(*), WHERE result = 'FAILED') WHERE entityGuid IN ('${newrelic_synthetics_monitor.api_health.id}')"
  }

  critical {
    operator              = "above"
    threshold             = 0
    threshold_duration    = 60
    threshold_occurrences = "at_least_once"
  }
}

resource "newrelic_one_dashboard" "operations" {
  name        = local.operations_dashboard_name
  description = "Indicadores de negocio, API e Kubernetes da Oficina Mecanica na AWS."
  permissions = "public_read_only"

  page {
    name        = "Negocio"
    description = "Volume de ordens, tempo por etapa e falhas de processamento."

    widget_line {
      title  = "Volume diario de ordens de servico"
      row    = 1
      column = 1
      width  = 6
      height = 3

      nrql_query {
        query = "FROM Log SELECT uniqueCount(ordemServicoId) AS 'Ordens criadas' WHERE entity.name = '${var.new_relic_app_name}' AND evento = 'OrdemServicoCriada' TIMESERIES 1 day"
      }
    }

    widget_bar {
      title  = "Tempo medio por etapa (minutos)"
      row    = 1
      column = 7
      width  = 6
      height = 3

      nrql_query {
        query = "FROM Log SELECT average(duracaoStatusSegundos) / 60 AS 'Tempo medio (min)' WHERE entity.name = '${var.new_relic_app_name}' AND evento = 'OrdemServicoStatusAlterado' AND statusAnterior IN ('EmDiagnostico', 'EmExecucao', 'Finalizada') FACET cases(WHERE statusAnterior = 'EmDiagnostico' AS 'Diagnostico', WHERE statusAnterior = 'EmExecucao' AS 'Execucao', WHERE statusAnterior = 'Finalizada' AS 'Finalizacao')"
      }
    }

    widget_table {
      title  = "Erros e falhas nas integracoes"
      row    = 4
      column = 1
      width  = 12
      height = 4

      nrql_query {
        query = "FROM TransactionError SELECT count(*) AS 'Ocorrencias' WHERE appName = '${var.new_relic_app_name}' FACET transactionName, error.class LIMIT 20"
      }
    }
  }

  page {
    name        = "Operacao"
    description = "Latencia, disponibilidade, erros e consumo do cluster Kubernetes."

    widget_line {
      title  = "Latencia da API"
      row    = 1
      column = 1
      width  = 6
      height = 3

      nrql_query {
        query = "FROM Transaction SELECT average(duration) * 1000 AS 'Media (ms)', percentile(duration, 95) * 1000 AS 'p95 (ms)' WHERE appName = '${var.new_relic_app_name}' TIMESERIES"
      }
    }

    widget_billboard {
      title  = "Taxa de respostas HTTP 5xx"
      row    = 1
      column = 7
      width  = 3
      height = 3

      nrql_query {
        query = "FROM Transaction SELECT percentage(count(*), WHERE http.statusCode >= 500) AS 'Taxa 5xx (%)' WHERE appName = '${var.new_relic_app_name}'"
      }
    }

    widget_billboard {
      title  = "Uptime do healthcheck"
      row    = 1
      column = 10
      width  = 3
      height = 3

      nrql_query {
        query = "FROM SyntheticCheck SELECT percentage(count(*), WHERE result = 'SUCCESS') AS 'Uptime (%)' WHERE monitorName = '${local.synthetic_healthcheck_name}'"
      }
    }

    widget_line {
      title  = "CPU dos pods da API"
      row    = 4
      column = 1
      width  = 6
      height = 3

      nrql_query {
        query = "FROM K8sContainerSample SELECT average(cpuCoresUtilization) AS 'CPU (%)' WHERE clusterName = '${data.terraform_remote_state.kubernetes_cluster.outputs.eks_cluster_name}' AND namespaceName = '${var.namespace}' AND containerName = 'api' FACET podName TIMESERIES"
      }
    }

    widget_line {
      title  = "Memoria dos pods da API"
      row    = 4
      column = 7
      width  = 6
      height = 3

      nrql_query {
        query = "FROM K8sContainerSample SELECT average(memoryWorkingSetUtilization) AS 'Memoria (%)' WHERE clusterName = '${data.terraform_remote_state.kubernetes_cluster.outputs.eks_cluster_name}' AND namespaceName = '${var.namespace}' AND containerName = 'api' FACET podName TIMESERIES"
      }
    }

    widget_log_table {
      title  = "Logs de erro com correlacao"
      row    = 7
      column = 1
      width  = 12
      height = 4

      nrql_query {
        query = "FROM Log SELECT * WHERE entity.name = '${var.new_relic_app_name}' AND (level = 'ERROR' OR level = 'Error') LIMIT 100"
      }
    }
  }

  depends_on = [newrelic_synthetics_monitor.api_health]
}

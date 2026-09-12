locals {
  synthetic_healthcheck_name = "Oficina Mecanica API - AWS - Health"
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

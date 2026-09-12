locals {
  new_relic_namespace        = "newrelic"
  new_relic_release_name     = "newrelic-bundle"
  synthetic_healthcheck_name = "Oficina Mecanica API - AWS - Health"
}

data "newrelic_alert_policy" "monitoring" {
  name = var.new_relic_alert_policy_name
}

resource "kubernetes_namespace_v1" "new_relic" {
  metadata {
    name = local.new_relic_namespace
  }
}

resource "kubernetes_secret_v1" "new_relic_license" {
  metadata {
    name      = "newrelic-license"
    namespace = kubernetes_namespace_v1.new_relic.metadata[0].name
  }

  type = "Opaque"

  data = {
    licenseKey = var.new_relic_license_key
  }
}

resource "helm_release" "new_relic" {
  name       = local.new_relic_release_name
  namespace  = kubernetes_namespace_v1.new_relic.metadata[0].name
  repository = "https://helm-charts.newrelic.com"
  chart      = "nri-bundle"
  version    = "8.0.12"

  atomic          = true
  cleanup_on_fail = true
  timeout         = 600
  wait            = true
  wait_for_jobs   = true

  values = [
    yamlencode({
      global = {
        cluster                = data.terraform_remote_state.platform.outputs.eks_cluster_name
        customSecretName       = kubernetes_secret_v1.new_relic_license.metadata[0].name
        customSecretLicenseKey = "licenseKey"
        lowDataMode            = true
        customAttributes = {
          environment = "aws"
          project     = "oficina-mecanica"
        }
      }

      "newrelic-infrastructure" = {
        enabled = true
      }

      "nri-metadata-injection" = {
        enabled = true
      }

      "kube-state-metrics" = {
        enabled = true
      }

      "nri-prometheus" = {
        enabled = false
      }

      "nri-kube-events" = {
        enabled = false
      }

      "newrelic-logging" = {
        enabled = false
      }

      "newrelic-pixie" = {
        enabled = false
      }

      "k8s-agents-operator" = {
        enabled = false
      }
    })
  ]

  depends_on = [kubernetes_secret_v1.new_relic_license]
}

resource "newrelic_synthetics_monitor" "api_health" {
  status           = "ENABLED"
  name             = local.synthetic_healthcheck_name
  period           = "EVERY_MINUTE"
  uri              = "http://${data.kubernetes_service_v1.kong_proxy.status[0].load_balancer[0].ingress[0].hostname}/health"
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
    kubernetes_ingress_v1.api_gateway,
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

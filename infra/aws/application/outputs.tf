output "namespace" {
  description = "Namespace Kubernetes da aplicacao."
  value       = kubernetes_namespace_v1.application.metadata[0].name
}

output "api_url" {
  description = "URL publica da API, exposta exclusivamente pelo Kong Gateway."
  value       = data.terraform_remote_state.kubernetes_addons.outputs.api_gateway_url
}

output "admin_username" {
  description = "Usuario administrador inicial."
  value       = var.admin_username
}

output "admin_password" {
  description = "Senha do administrador inicial. Consulte apenas quando necessario."
  value       = random_password.admin.result
  sensitive   = true
}

output "jwt_secret" {
  description = "Segredo compartilhado com os emissores e validadores de JWT."
  value       = random_password.jwt_secret.result
  sensitive   = true
}

output "new_relic_app_name" {
  description = "Nome da entidade APM criada quando a API enviar a primeira telemetria."
  value       = var.new_relic_app_name
}

output "new_relic_cluster_name" {
  description = "Nome usado para identificar o cluster EKS no New Relic."
  value       = data.terraform_remote_state.kubernetes_cluster.outputs.eks_cluster_name
}

output "new_relic_health_monitor_name" {
  description = "Nome do monitor sintetico que consulta o endpoint de healthcheck."
  value       = newrelic_synthetics_monitor.api_health.name
}

output "new_relic_dashboard_name" {
  description = "Nome do dashboard operacional criado no New Relic."
  value       = newrelic_one_dashboard.operations.name
}

output "new_relic_dashboard_url" {
  description = "Link direto para o dashboard operacional no New Relic."
  value       = newrelic_one_dashboard.operations.permalink
}

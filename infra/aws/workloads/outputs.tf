output "namespace" {
  description = "Namespace Kubernetes da aplicacao."
  value       = kubernetes_namespace_v1.application.metadata[0].name
}

output "load_balancer_hostname" {
  description = "Hostname publico criado pelo Service LoadBalancer."
  value       = try(kubernetes_service_v1.api.status[0].load_balancer[0].ingress[0].hostname, null)
}

output "api_url" {
  description = "URL publica da API."
  value       = try("http://${kubernetes_service_v1.api.status[0].load_balancer[0].ingress[0].hostname}", null)
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

output "new_relic_app_name" {
  description = "Nome da entidade APM criada quando a API enviar a primeira telemetria."
  value       = var.new_relic_app_name
}

output "new_relic_cluster_name" {
  description = "Nome usado para identificar o cluster EKS no New Relic."
  value       = data.terraform_remote_state.platform.outputs.eks_cluster_name
}

output "new_relic_health_monitor_name" {
  description = "Nome do monitor sintetico que consulta o endpoint de healthcheck."
  value       = newrelic_synthetics_monitor.api_health.name
}

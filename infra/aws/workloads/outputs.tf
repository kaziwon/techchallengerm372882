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

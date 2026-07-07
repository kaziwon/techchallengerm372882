output "cluster_name" {
  description = "Nome do cluster Kubernetes local provisionado pelo Terraform."
  value       = kind_cluster.oficina_mecanica.name
}

output "kubeconfig_path" {
  description = "Caminho do kubeconfig gerado para acessar o cluster kind."
  value       = kind_cluster.oficina_mecanica.kubeconfig_path
}

output "namespace" {
  description = "Namespace Kubernetes da aplicacao."
  value       = "oficina-mecanica"
}

output "api_url" {
  description = "URL local da API publicada pelo cluster kind."
  value       = "http://localhost:18080/swagger/index.html"
}

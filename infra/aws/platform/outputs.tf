output "aws_region" {
  description = "Regiao em que a plataforma foi criada."
  value       = var.aws_region
}

output "vpc_id" {
  description = "ID da VPC da aplicacao."
  value       = aws_vpc.main.id
}

output "public_subnet_ids" {
  description = "IDs das sub-redes publicas do EKS."
  value       = values(aws_subnet.public)[*].id
}

output "database_subnet_ids" {
  description = "IDs das sub-redes privadas do RDS."
  value       = values(aws_subnet.database)[*].id
}

output "database_security_group_id" {
  description = "Security Group do RDS usado para liberar consumidores internos."
  value       = aws_security_group.database.id
}

output "ecr_repository_url" {
  description = "Endereco do repositorio privado da imagem da API."
  value       = aws_ecr_repository.api.repository_url
}

output "eks_cluster_name" {
  description = "Nome do cluster EKS."
  value       = aws_eks_cluster.main.name
}

output "eks_node_group_name" {
  description = "Nome do grupo de nodes gerenciado pelo EKS."
  value       = aws_eks_node_group.main.node_group_name
}

output "eks_endpoint" {
  description = "Endpoint da API Kubernetes do cluster."
  value       = aws_eks_cluster.main.endpoint
}

output "database_address" {
  description = "Endereco privado da instancia RDS."
  value       = aws_db_instance.database.address
}

output "database_port" {
  description = "Porta MySQL da instancia RDS."
  value       = aws_db_instance.database.port
}

output "database_name" {
  description = "Nome do schema inicial do MySQL."
  value       = aws_db_instance.database.db_name
}

output "database_username" {
  description = "Usuario inicial do MySQL."
  value       = aws_db_instance.database.username
  sensitive   = true
}

output "database_password" {
  description = "Senha gerada para o usuario inicial do MySQL."
  value       = random_password.database.result
  sensitive   = true
}

output "update_kubeconfig_command" {
  description = "Comando local para configurar o kubectl para este cluster."
  value       = "aws eks update-kubeconfig --region ${var.aws_region} --name ${aws_eks_cluster.main.name} --profile academy"
}

output "state_bucket_name" {
  description = "Nome do bucket S3 que armazenara os estados remotos do Terraform."
  value       = aws_s3_bucket.terraform_state.id
}

output "state_bucket_arn" {
  description = "ARN do bucket S3 de estados do Terraform."
  value       = aws_s3_bucket.terraform_state.arn
}

output "platform_backend_config" {
  description = "Configuracao que sera usada pelo backend do Terraform da plataforma AWS."
  value = {
    bucket       = aws_s3_bucket.terraform_state.id
    key          = "oficina-mecanica/platform/terraform.tfstate"
    region       = var.aws_region
    use_lockfile = true
  }
}

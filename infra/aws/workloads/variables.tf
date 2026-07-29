variable "aws_region" {
  description = "Regiao em que o cluster EKS foi criado."
  type        = string
  default     = "us-east-1"
}

variable "state_bucket_name" {
  description = "Bucket S3 que armazena os estados remotos do Terraform."
  type        = string

  validation {
    condition     = length(trimspace(var.state_bucket_name)) > 0
    error_message = "O nome do bucket de estado nao pode ser vazio."
  }
}

variable "api_image" {
  description = "URI completa e imutavel da imagem da API no Amazon ECR."
  type        = string

  validation {
    condition     = length(trimspace(var.api_image)) > 0
    error_message = "A URI da imagem da API nao pode ser vazia."
  }
}

variable "namespace" {
  description = "Namespace Kubernetes da aplicacao."
  type        = string
  default     = "oficina-mecanica"
}

variable "admin_username" {
  description = "Usuario administrador inicial da API."
  type        = string
  default     = "admin"
}

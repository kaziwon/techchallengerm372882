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

variable "new_relic_license_key" {
  description = "Chave de ingestao usada pelo agente APM embarcado na imagem da API."
  type        = string
  sensitive   = true

  validation {
    condition     = length(trimspace(var.new_relic_license_key)) > 0
    error_message = "A license key do New Relic nao pode ser vazia."
  }
}

variable "new_relic_app_name" {
  description = "Nome da entidade APM da API executada na AWS."
  type        = string
  default     = "oficina-mecanica-api-aws"
}

variable "new_relic_alert_policy_name" {
  description = "Policy existente no New Relic que recebe as condicoes gerenciadas pelo Terraform."
  type        = string
  default     = "Oficina Mecanica - Monitoramento"
}

variable "new_relic_synthetics_location" {
  description = "Localizacao publica usada pelo monitor sintetico do healthcheck."
  type        = string
  default     = "US_EAST_1"
}

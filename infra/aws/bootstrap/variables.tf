variable "aws_region" {
  description = "Regiao AWS utilizada pelo ambiente da AWS Academy."
  type        = string
  default     = "us-east-1"
}

variable "project_name" {
  description = "Nome usado para identificar os recursos do projeto."
  type        = string
  default     = "oficina-mecanica"
}

variable "environment" {
  description = "Ambiente ao qual os recursos pertencem."
  type        = string
  default     = "academy"
}

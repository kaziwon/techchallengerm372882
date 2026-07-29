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

variable "vpc_cidr" {
  description = "Faixa de enderecos IP privados da VPC."
  type        = string
  default     = "10.0.0.0/16"
}

variable "availability_zones" {
  description = "Zonas de disponibilidade usadas pela plataforma."
  type        = list(string)
  default     = ["us-east-1a", "us-east-1b"]

  validation {
    condition     = length(var.availability_zones) == 2
    error_message = "Informe exatamente duas zonas de disponibilidade."
  }
}

variable "public_subnet_cidrs" {
  description = "Faixas IP das sub-redes publicas do EKS e Load Balancer."
  type        = list(string)
  default     = ["10.0.0.0/24", "10.0.1.0/24"]

  validation {
    condition     = length(var.public_subnet_cidrs) == 2
    error_message = "Informe exatamente duas faixas para sub-redes publicas."
  }
}

variable "database_subnet_cidrs" {
  description = "Faixas IP das sub-redes privadas do RDS."
  type        = list(string)
  default     = ["10.0.10.0/24", "10.0.11.0/24"]

  validation {
    condition     = length(var.database_subnet_cidrs) == 2
    error_message = "Informe exatamente duas faixas para sub-redes do banco."
  }
}

variable "cluster_name" {
  description = "Nome do cluster Amazon EKS."
  type        = string
  default     = "oficina-mecanica-eks"
}

variable "kubernetes_version" {
  description = "Versao Kubernetes em suporte padrao no EKS."
  type        = string
  default     = "1.36"
}

variable "node_instance_types" {
  description = "Tipos de instancia permitidos para os nodes do EKS."
  type        = list(string)
  default     = ["t3.medium"]
}

variable "node_min_size" {
  description = "Quantidade minima de nodes no grupo gerenciado."
  type        = number
  default     = 1
}

variable "node_desired_size" {
  description = "Quantidade inicial de nodes no grupo gerenciado."
  type        = number
  default     = 1
}

variable "node_max_size" {
  description = "Quantidade maxima de nodes permitida no grupo gerenciado."
  type        = number
  default     = 2
}

variable "database_identifier" {
  description = "Identificador da instancia RDS."
  type        = string
  default     = "oficina-mecanica-mysql"
}

variable "database_name" {
  description = "Nome do schema inicial criado no MySQL."
  type        = string
  default     = "oficina_mecanica"
}

variable "database_username" {
  description = "Usuario administrador inicial do MySQL."
  type        = string
  default     = "oficina_user"
}

variable "database_engine_version" {
  description = "Versao do MySQL disponibilizada pelo RDS."
  type        = string
  default     = "8.4.10"
}

variable "database_instance_class" {
  description = "Classe de menor custo permitida para a instancia RDS."
  type        = string
  default     = "db.t3.micro"
}

variable "database_allocated_storage" {
  description = "Armazenamento do RDS em GiB."
  type        = number
  default     = 20
}

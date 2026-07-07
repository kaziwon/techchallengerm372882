variable "cluster_name" {
  description = "Nome do cluster Kubernetes local criado com kind."
  type        = string
  default     = "oficina-mecanica"
}

variable "api_image" {
  description = "Imagem Docker local da API que sera carregada no cluster kind."
  type        = string
  default     = "oficina-mecanica-api:local"
}

variable "kubeconfig_file" {
  description = "Arquivo local em que o provider kind gravara o kubeconfig do cluster."
  type        = string
  default     = ".terraform/oficina-mecanica-kubeconfig"
}

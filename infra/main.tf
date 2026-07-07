locals {
  kubeconfig_path = abspath("${path.module}/${var.kubeconfig_file}")

  namespace_files                = fileset("${path.module}/manifests/00-namespaces", "*.yaml")
  config_files                   = fileset("${path.module}/manifests/01-config", "*.yaml")
  database_storage_files         = fileset("${path.module}/manifests/02-database-storage", "*.yaml")
  database_files                 = fileset("${path.module}/manifests/03-database", "*.yaml")
  observability_rbac_files       = fileset("${path.module}/manifests/04-observability-rbac", "*.yaml")
  observability_service_files    = fileset("${path.module}/manifests/05-observability-service", "*.yaml")
  observability_deployment_files = fileset("${path.module}/manifests/06-observability-deployment", "*.yaml")
  observability_apiservice_files = fileset("${path.module}/manifests/07-observability-apiservice", "*.yaml")
  api_files                      = fileset("${path.module}/manifests/08-api", "*.yaml")
  autoscaling_files              = fileset("${path.module}/manifests/09-autoscaling", "*.yaml")
}

provider "kind" {}

resource "kind_cluster" "oficina_mecanica" {
  name            = var.cluster_name
  wait_for_ready  = true
  kubeconfig_path = local.kubeconfig_path

  kind_config {
    kind        = "Cluster"
    api_version = "kind.x-k8s.io/v1alpha4"

    node {
      role = "control-plane"

      extra_port_mappings {
        container_port = 30080
        host_port      = 18080
        protocol       = "TCP"
      }
    }
  }
}

resource "terraform_data" "load_api_image" {
  input = var.api_image

  provisioner "local-exec" {
    command = <<-EOT
      set -e
      mkdir -p .terraform/image-cache
      docker save ${var.api_image} -o .terraform/image-cache/api-image.tar
      docker cp .terraform/image-cache/api-image.tar ${kind_cluster.oficina_mecanica.name}-control-plane:/api-image.tar
      docker exec ${kind_cluster.oficina_mecanica.name}-control-plane ctr -n k8s.io images import /api-image.tar
      docker exec ${kind_cluster.oficina_mecanica.name}-control-plane rm /api-image.tar
    EOT
  }

  depends_on = [kind_cluster.oficina_mecanica]
}

provider "kubectl" {
  config_path       = kind_cluster.oficina_mecanica.kubeconfig_path
  config_context    = "kind-${kind_cluster.oficina_mecanica.name}"
  load_config_file  = true
  apply_retry_count = 10
}

resource "kubectl_manifest" "namespaces" {
  for_each  = local.namespace_files
  yaml_body = file("${path.module}/manifests/00-namespaces/${each.value}")

  depends_on = [kind_cluster.oficina_mecanica]
}

resource "kubectl_manifest" "config" {
  for_each         = local.config_files
  yaml_body        = file("${path.module}/manifests/01-config/${each.value}")
  sensitive_fields = ["stringData"]

  depends_on = [kubectl_manifest.namespaces]
}

resource "kubectl_manifest" "database_storage" {
  for_each  = local.database_storage_files
  yaml_body = file("${path.module}/manifests/02-database-storage/${each.value}")

  depends_on = [kubectl_manifest.config]
}

resource "kubectl_manifest" "database" {
  for_each  = local.database_files
  yaml_body = file("${path.module}/manifests/03-database/${each.value}")

  depends_on = [kubectl_manifest.database_storage]
}

resource "kubectl_manifest" "observability_rbac" {
  for_each  = local.observability_rbac_files
  yaml_body = file("${path.module}/manifests/04-observability-rbac/${each.value}")

  depends_on = [kubectl_manifest.namespaces]
}

resource "kubectl_manifest" "observability_service" {
  for_each  = local.observability_service_files
  yaml_body = file("${path.module}/manifests/05-observability-service/${each.value}")

  depends_on = [kubectl_manifest.observability_rbac]
}

resource "kubectl_manifest" "observability_deployment" {
  for_each  = local.observability_deployment_files
  yaml_body = file("${path.module}/manifests/06-observability-deployment/${each.value}")

  depends_on = [kubectl_manifest.observability_service]
}

resource "kubectl_manifest" "observability_apiservice" {
  for_each         = local.observability_apiservice_files
  yaml_body        = file("${path.module}/manifests/07-observability-apiservice/${each.value}")
  wait_for_rollout = false

  depends_on = [kubectl_manifest.observability_deployment]
}

resource "kubectl_manifest" "api" {
  for_each  = local.api_files
  yaml_body = file("${path.module}/manifests/08-api/${each.value}")

  depends_on = [
    terraform_data.load_api_image,
    kubectl_manifest.database,
  ]
}

resource "kubectl_manifest" "autoscaling" {
  for_each  = local.autoscaling_files
  yaml_body = file("${path.module}/manifests/09-autoscaling/${each.value}")

  depends_on = [
    kubectl_manifest.api,
    kubectl_manifest.observability_apiservice,
  ]
}

locals {
  app_name = "oficina-mecanica-api"

  labels = {
    app = local.app_name
  }

  database_host     = data.terraform_remote_state.database.outputs.database_address
  database_port     = tostring(data.terraform_remote_state.database.outputs.database_port)
  database_name     = data.terraform_remote_state.database.outputs.database_name
  database_username = data.terraform_remote_state.database.outputs.database_username
  database_password = data.terraform_remote_state.database.outputs.database_password

  connection_string = join("", [
    "server=${local.database_host};",
    "port=${local.database_port};",
    "database=${local.database_name};",
    "user=${local.database_username};",
    "password=${local.database_password};",
    "SslMode=Required",
  ])
}

resource "random_password" "jwt_secret" {
  length  = 64
  special = false
}

resource "random_password" "admin" {
  length           = 20
  special          = true
  override_special = "!#$%&*()-_+?"
}

resource "kubernetes_namespace_v1" "application" {
  metadata {
    name = var.namespace
  }
}

resource "kubernetes_config_map_v1" "application" {
  metadata {
    name      = "oficina-mecanica-config"
    namespace = kubernetes_namespace_v1.application.metadata[0].name
  }

  data = {
    ASPNETCORE_ENVIRONMENT                                        = "Development"
    ASPNETCORE_URLS                                               = "http://+:8080"
    JwtSettings__Issuer                                           = "OficinaMecanica.Api"
    JwtSettings__Audience                                         = "OficinaMecanica.Api"
    CORECLR_ENABLE_PROFILING                                      = "1"
    NEW_RELIC_APP_NAME                                            = var.new_relic_app_name
    NEW_RELIC_DISTRIBUTED_TRACING_ENABLED                         = "true"
    NEW_RELIC_APPLICATION_LOGGING_ENABLED                         = "true"
    NEW_RELIC_APPLICATION_LOGGING_FORWARDING_ENABLED              = "true"
    NEW_RELIC_APPLICATION_LOGGING_FORWARDING_CONTEXT_DATA_ENABLED = "true"
    NEW_RELIC_APPLICATION_LOGGING_LOCAL_DECORATING_ENABLED        = "false"
    NEW_RELIC_LABELS                                              = "environment:aws"
  }
}

resource "kubernetes_secret_v1" "application" {
  metadata {
    name      = "oficina-mecanica-secret"
    namespace = kubernetes_namespace_v1.application.metadata[0].name
  }

  type = "Opaque"

  data = {
    ConnectionStrings__DefaultConnection = local.connection_string
    JwtSettings__SecretKey               = random_password.jwt_secret.result
    AdminUserSettings__Username          = var.admin_username
    AdminUserSettings__Password          = random_password.admin.result
    DATABASE_HOST                        = local.database_host
    DATABASE_PORT                        = local.database_port
    DATABASE_NAME                        = local.database_name
    DATABASE_USER                        = local.database_username
    DATABASE_PASSWORD                    = local.database_password
  }
}

resource "kubernetes_secret_v1" "application_observability" {
  metadata {
    name      = "oficina-mecanica-observability-secret"
    namespace = kubernetes_namespace_v1.application.metadata[0].name
  }

  type = "Opaque"

  data = {
    NEW_RELIC_LICENSE_KEY = var.new_relic_license_key
  }
}

resource "kubernetes_deployment_v1" "api" {
  metadata {
    name      = local.app_name
    namespace = kubernetes_namespace_v1.application.metadata[0].name
    labels    = local.labels
  }

  wait_for_rollout = true

  spec {
    replicas = 1

    selector {
      match_labels = local.labels
    }

    strategy {
      type = "RollingUpdate"

      rolling_update {
        max_surge       = "1"
        max_unavailable = "0"
      }
    }

    template {
      metadata {
        labels = local.labels
      }

      spec {
        init_container {
          name  = "wait-for-rds"
          image = "mysql:8.4"
          command = [
            "sh",
            "-c",
            "until mysqladmin ping -h \"$DATABASE_HOST\" -P \"$DATABASE_PORT\" -u\"$DATABASE_USER\" -p\"$DATABASE_PASSWORD\" --silent; do echo 'waiting for RDS'; sleep 5; done",
          ]

          env_from {
            secret_ref {
              name = kubernetes_secret_v1.application.metadata[0].name
            }
          }

          resources {
            requests = {
              cpu    = "25m"
              memory = "64Mi"
            }
            limits = {
              cpu    = "100m"
              memory = "128Mi"
            }
          }
        }

        container {
          name              = "api"
          image             = var.api_image
          image_pull_policy = "Always"

          port {
            name           = "http"
            container_port = 8080
            protocol       = "TCP"
          }

          env_from {
            config_map_ref {
              name = kubernetes_config_map_v1.application.metadata[0].name
            }
          }

          env_from {
            secret_ref {
              name = kubernetes_secret_v1.application.metadata[0].name
            }
          }

          env_from {
            secret_ref {
              name = kubernetes_secret_v1.application_observability.metadata[0].name
            }
          }

          readiness_probe {
            http_get {
              path   = "/health"
              port   = "http"
              scheme = "HTTP"
            }

            initial_delay_seconds = 10
            period_seconds        = 5
            timeout_seconds       = 3
            failure_threshold     = 12
          }

          liveness_probe {
            http_get {
              path   = "/health"
              port   = "http"
              scheme = "HTTP"
            }

            initial_delay_seconds = 30
            period_seconds        = 10
            timeout_seconds       = 3
            failure_threshold     = 6
          }

          resources {
            requests = {
              cpu    = "100m"
              memory = "256Mi"
            }
            limits = {
              cpu    = "500m"
              memory = "512Mi"
            }
          }
        }

        termination_grace_period_seconds = 30
      }
    }
  }
}

resource "kubernetes_service_v1" "api" {
  metadata {
    name      = local.app_name
    namespace = kubernetes_namespace_v1.application.metadata[0].name
  }

  spec {
    selector = local.labels
    type     = "ClusterIP"

    port {
      name        = "http"
      port        = 80
      target_port = "http"
      protocol    = "TCP"
    }
  }
}

resource "kubernetes_horizontal_pod_autoscaler_v2" "api" {
  metadata {
    name      = local.app_name
    namespace = kubernetes_namespace_v1.application.metadata[0].name
  }

  spec {
    min_replicas = 1
    max_replicas = 3

    scale_target_ref {
      api_version = "apps/v1"
      kind        = "Deployment"
      name        = kubernetes_deployment_v1.api.metadata[0].name
    }

    metric {
      type = "Resource"

      resource {
        name = "cpu"

        target {
          type                = "Utilization"
          average_utilization = 70
        }
      }
    }

    metric {
      type = "Resource"

      resource {
        name = "memory"

        target {
          type                = "Utilization"
          average_utilization = 80
        }
      }
    }
  }

  depends_on = [kubernetes_service_v1.api]
}

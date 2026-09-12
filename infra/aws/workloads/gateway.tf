locals {
  kong_namespace    = "kong"
  kong_release_name = "kong"
}

resource "kubernetes_namespace_v1" "kong" {
  metadata {
    name = local.kong_namespace
  }
}

resource "helm_release" "kong" {
  name       = local.kong_release_name
  namespace  = kubernetes_namespace_v1.kong.metadata[0].name
  repository = "https://charts.konghq.com"
  chart      = "ingress"
  version    = "0.24.0"

  atomic          = true
  cleanup_on_fail = true
  timeout         = 600
  wait            = true
  wait_for_jobs   = true

  values = [
    yamlencode({
      controller = {
        ingressController = {
          resources = {
            requests = {
              cpu    = "100m"
              memory = "128Mi"
            }
            limits = {
              cpu    = "250m"
              memory = "256Mi"
            }
          }
        }
      }

      gateway = {
        deployment = {
          replicas = 1
        }

        proxy = {
          type = "LoadBalancer"
          annotations = {
            "service.beta.kubernetes.io/aws-load-balancer-type" = "nlb"
          }
          http = {
            enabled = true
          }
          tls = {
            enabled = false
          }
        }

        resources = {
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
    })
  ]

  depends_on = [kubernetes_namespace_v1.kong]
}

data "kubernetes_service_v1" "kong_proxy" {
  metadata {
    name      = "${local.kong_release_name}-gateway-proxy"
    namespace = kubernetes_namespace_v1.kong.metadata[0].name
  }

  depends_on = [helm_release.kong]
}

resource "helm_release" "kong_auth" {
  name      = "oficina-mecanica-kong-auth"
  namespace = kubernetes_namespace_v1.application.metadata[0].name
  chart     = "${path.module}/charts/kong-auth"

  atomic          = true
  cleanup_on_fail = true
  timeout         = 300
  wait            = true

  values = [
    yamlencode({
      jwt = {
        issuer = "OficinaMecanica.Api"
        secret = random_password.jwt_secret.result
      }
    })
  ]

  depends_on = [
    helm_release.kong,
    kubernetes_namespace_v1.application,
  ]
}

moved {
  from = kubernetes_ingress_v1.api_gateway
  to   = kubernetes_ingress_v1.api_public
}

resource "kubernetes_ingress_v1" "api_public" {
  metadata {
    name      = "oficina-mecanica-api-public"
    namespace = kubernetes_namespace_v1.application.metadata[0].name

    annotations = {
      "konghq.com/strip-path" = "false"
    }
  }

  spec {
    ingress_class_name = "kong"

    rule {
      http {
        path {
          path      = "/"
          path_type = "Prefix"

          backend {
            service {
              name = kubernetes_service_v1.api.metadata[0].name

              port {
                name = "http"
              }
            }
          }
        }
      }
    }
  }

  depends_on = [
    helm_release.kong,
    kubernetes_service_v1.api,
  ]
}

resource "kubernetes_ingress_v1" "api_protected" {
  metadata {
    name      = "oficina-mecanica-api-protected"
    namespace = kubernetes_namespace_v1.application.metadata[0].name

    annotations = {
      "konghq.com/plugins"        = "oficina-mecanica-jwt"
      "konghq.com/regex-priority" = "100"
      "konghq.com/strip-path"     = "false"
    }
  }

  spec {
    ingress_class_name = "kong"

    rule {
      http {
        path {
          path      = "/~(?i)^/api/(clientes|veiculos|servicos|pecas)(?:/.*)?$"
          path_type = "ImplementationSpecific"

          backend {
            service {
              name = kubernetes_service_v1.api.metadata[0].name

              port {
                name = "http"
              }
            }
          }
        }

        path {
          path      = "/~(?i)^/api/ordensservico(?:/?|/tempo-medio-execucao/?|/[0-9a-f-]{36}/?|/[0-9a-f-]{36}/(?:iniciar-diagnostico|enviar-orcamento|aprovar-orcamento|recusar-orcamento|cancelar|finalizar|entregar)/?)$"
          path_type = "ImplementationSpecific"

          backend {
            service {
              name = kubernetes_service_v1.api.metadata[0].name

              port {
                name = "http"
              }
            }
          }
        }
      }
    }
  }

  depends_on = [
    helm_release.kong_auth,
    kubernetes_service_v1.api,
  ]
}

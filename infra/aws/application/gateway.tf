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
    data.terraform_remote_state.kubernetes_addons,
    kubernetes_namespace_v1.application,
  ]
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
    data.terraform_remote_state.kubernetes_addons,
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
          path      = "/~/(?i:api/(clientes|veiculos|servicos|pecas)(?:/.*)?)$"
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
          path      = "/~/(?i:api/ordensservico(?:/?|/tempo-medio-execucao/?|/[0-9a-f-]{36}/?|/[0-9a-f-]{36}/(?:iniciar-diagnostico|enviar-orcamento|aprovar-orcamento|recusar-orcamento|cancelar|finalizar|entregar)/?))$"
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

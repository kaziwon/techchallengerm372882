# Documentacao da Fase 3

Este diretorio concentra a documentacao arquitetural da solucao, conforme a
orientacao de manter os documentos completos no repositorio da aplicacao
principal.

## Documentos

- [Componentes de nuvem](architecture/componentes-cloud.md)
- [Diagramas de sequencia](architecture/sequencias.md)
- [Banco de dados e diagrama ER](architecture/banco-de-dados.md)
- [Monitoramento e observabilidade](observabilidade.md)
- [Checklist da entrega final](entrega-final.md)
- [RFC-001: plataforma AWS](rfcs/RFC-001-plataforma-aws.md)
- [RFC-002: banco relacional gerenciado](rfcs/RFC-002-banco-relacional-gerenciado.md)
- [RFC-003: autenticacao serverless por CPF](rfcs/RFC-003-autenticacao-serverless-cpf.md)
- [ADR-001: comunicacao entre componentes](adrs/ADR-001-comunicacao-entre-componentes.md)
- [ADR-002: escalabilidade horizontal](adrs/ADR-002-escalabilidade-horizontal.md)
- [ADR-003: Kong em modo DB-less](adrs/ADR-003-kong-db-less.md)

## Rastreabilidade dos requisitos

| Requisito | Implementacao | Evidencia principal |
|---|---|---|
| API Gateway | Kong no EKS, exposto por Network Load Balancer | `infra/aws/application/gateway.tf` e repositorio de Kubernetes |
| Autenticacao por CPF | Lambda consulta cliente ativo e emite JWT | repositorio `oficina-mecanica-autenticacao-lambda` |
| Rotas protegidas | Plugin JWT do Kong e autenticacao JWT da API | `infra/aws/application/charts/kong-auth` e `Program.cs` |
| Banco gerenciado | Amazon RDS for MySQL em sub-redes privadas | repositorio `oficina-mecanica-infra-banco` |
| Kubernetes escalavel | Amazon EKS, Metrics Server e HPA | repositorio de Kubernetes e `infra/aws/application/main.tf` |
| Infraestrutura como codigo | Terraform dividido entre quatro states | os quatro repositorios da Fase 3 |
| CI/CD | CI em PR e CD manual de producao | `.github/workflows` de cada repositorio |
| Observabilidade | APM, logs, Kubernetes, Synthetics, alertas e dashboard | `infra/aws/application/observability.tf` |
| Correlacao | `X-Correlation-ID`, `traceId` e `spanId` nos logs JSON | `Infrastructure/Observability` e `Program.cs` |
| Documentacao arquitetural | diagramas, RFCs e ADRs | este diretorio |

## Repositorios

1. [Banco e rede](https://github.com/kaziwon/oficina-mecanica-infra-banco)
2. [Plataforma Kubernetes](https://github.com/kaziwon/oficina-mecanica-infra-kubernetes)
3. [Aplicacao principal](https://github.com/kaziwon/techchallengerm372882)
4. [Autenticacao serverless](https://github.com/kaziwon/oficina-mecanica-autenticacao-lambda)

Os links do video e do documento final de entrega devem ser adicionados somente
depois da gravacao e da revisao final do ambiente de producao.

# Diagrama de componentes de nuvem

O diagrama mostra apenas servicos executados ou gerenciados em nuvem e as
integracoes externas necessarias para observar e consumir o sistema. A
organizacao interna da Clean Architecture esta documentada no README principal.

```mermaid
flowchart TB
    CLIENTE["Cliente ou operador"]
    NR["New Relic SaaS"]

    subgraph AWS["AWS Academy - us-east-1"]
        S3["Amazon S3\nStates do Terraform"]
        ECR["Amazon ECR\nImagem da API"]
        CW["Amazon CloudWatch Logs\nLogs da Lambda"]
        EKS["Amazon EKS\nControl plane gerenciado"]
        LAMBDAURL["Lambda Function URL\nEndpoint HTTPS gerenciado"]

        subgraph VPC["Amazon VPC"]
            subgraph PUBLICAS["Sub-redes publicas"]
                NLB["Network Load Balancer"]
                NODES["Managed Node Group"]
                KONG["Kong Ingress Controller"]
                API["API Oficina Mecanica\nDeployment + Service"]
                HPA["HPA + Metrics Server"]
                NRAGENT["New Relic Kubernetes"]
            end

            subgraph PRIVADAS["Sub-redes privadas"]
                LAMBDA["AWS Lambda\nAutenticacao por CPF"]
                RDS[("Amazon RDS MySQL")]
            end
        end
    end

    CLIENTE -->|"HTTPS: autenticar CPF"| LAMBDAURL
    LAMBDAURL --> LAMBDA
    LAMBDA -->|"SQL/TCP 3306"| RDS
    LAMBDA --> CW
    LAMBDA -. "JWT" .-> CLIENTE

    CLIENTE -->|"HTTP + Bearer JWT"| NLB
    NLB --> KONG
    KONG --> API
    API -->|"SQL/TCP 3306"| RDS
    ECR --> API
    EKS --> NODES
    NODES --> KONG
    NODES --> API
    HPA --> API
    NRAGENT --> NR
    API -->|"APM, traces e logs"| NR
    NR -->|"Synthetic /health"| NLB

    S3 -. "state remoto" .-> EKS
    S3 -. "state remoto" .-> RDS
    S3 -. "state remoto" .-> LAMBDA
```

## Responsabilidades

| Componente | Responsabilidade | Repositorio proprietario |
|---|---|---|
| S3 | Armazenar os states remotos separados do Terraform | infraestrutura de banco |
| VPC, sub-redes e Security Groups | Isolar rede e permitir somente os fluxos necessarios | infraestrutura de banco, Kubernetes e Lambda |
| RDS MySQL | Persistir os dados transacionais da oficina | infraestrutura de banco |
| EKS e Managed Node Group | Executar os workloads Kubernetes | infraestrutura Kubernetes |
| ECR | Armazenar a imagem imutavel da API | infraestrutura Kubernetes |
| Kong e Network Load Balancer | Formar o unico ponto de entrada da API e validar JWT | infraestrutura Kubernetes e aplicacao |
| API, Service e HPA | Executar os casos de uso e escalar os pods | aplicacao principal |
| Lambda e Function URL | Autenticar cliente ativo por CPF e emitir JWT | autenticacao serverless |
| CloudWatch Logs | Reter logs estruturados da Lambda por sete dias | autenticacao serverless |
| New Relic | APM, logs, traces, Kubernetes, uptime, alertas e dashboard | Kubernetes e aplicacao principal |

## Limites de rede

- O RDS possui `publicly_accessible = false` e fica nas sub-redes privadas.
- O Service da API e `ClusterIP`; ele nao possui endereco publico proprio.
- O Kong e o unico Service Kubernetes do tipo `LoadBalancer`.
- O Security Group do RDS aceita a porta 3306 somente dos Security Groups dos
  nodes do EKS e da Lambda.
- A Function URL e um endpoint HTTPS gerenciado fora das sub-redes. A funcao se
  conecta as sub-redes privadas para acessar o RDS; seu handler aceita somente
  `POST`, valida o CPF e nunca retorna informacoes de clientes inativos.

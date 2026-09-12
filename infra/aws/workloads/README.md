# Workloads Kubernetes na AWS

Este stack e executado depois da preparacao do bucket e do stack `platform`. Ele
le o estado da plataforma no S3 e publica no EKS apenas os componentes da aplicacao:

- namespace;
- ConfigMap;
- Secret;
- Deployment da API;
- Service interno `ClusterIP`;
- Horizontal Pod Autoscaler;
- Kong Gateway e Kong Ingress Controller;
- rotas publicas e protegidas da API;
- plugin JWT, Consumer e credencial do Kong;
- agente de infraestrutura do New Relic instalado pelo Helm;
- monitor sintetico do endpoint `/health`;
- condicoes de alerta para indisponibilidade e respostas 5xx nas ordens de servico.

O MySQL nao e criado como pod neste ambiente. A conexao da API aponta para a
instancia RDS privada criada pelo stack `platform`. O Metrics Server tambem e
gerenciado pela plataforma como add-on do EKS.

O chart oficial `kong/ingress` instala o Kong em modo DB-less. Apenas o proxy
do Kong usa um Service `LoadBalancer`; a API nao possui endereco publico proprio.
Uma rota com prefixo `/` preserva o caminho original e encaminha `/api`,
`/swagger` e `/health` para o Service interno da aplicacao. Rotas regex de maior
prioridade recebem o plugin JWT e exigem um token assinado pelo segredo
compartilhado. A consulta de OS por CPF e a notificacao externa de orcamento nao
casam com essas regex e permanecem publicas. Essa configuracao substitui o
cadastro manual de Services, Routes, Consumers e Plugins em uma interface
administrativa.

As senhas do banco, do JWT e do administrador e a license key do New Relic nao
ficam gravadas no Git. Elas ficam nos estados criptografados no S3 e nos Secrets
do Kubernetes.

O arquivo `observability.tf` instala o chart oficial `nri-bundle`. Ele envia CPU
e memoria dos nodes, pods e containers e injeta metadados Kubernetes no pod da
API. Os logs nao sao coletados uma segunda vez pelo chart: o proprio agente .NET
encaminha os logs JSON com `trace.id` e `span.id`.

O Terraform usa a policy existente `Oficina Mecanica - Monitoramento`. Dessa
forma, as duas condicoes criadas na AWS aproveitam o workflow de e-mail que ja
esta conectado a essa policy no New Relic.

## Configuracao no GitHub

Em `Settings -> Secrets and variables -> Actions`, configure:

- Secret `NEW_RELIC_LICENSE_KEY`: chave de ingestao entregue pelo instalador;
- Secret `NEW_RELIC_USER_API_KEY`: User API key, normalmente iniciada por `NRAK`;
- Variable `NEW_RELIC_ACCOUNT_ID`: numero da conta New Relic.

A license key permite que os agentes enviem telemetria. A User API key permite
que o Terraform crie e remova o monitor sintetico e as condicoes de alerta. Sao
credenciais diferentes.

## Execucao manual para diagnostico

O fluxo normal e automatizado pelo GitHub Actions. Para executar manualmente,
com a plataforma ja criada:

```bash
export AWS_PROFILE=academy
export TF_VAR_new_relic_license_key="<license-key>"
export NEW_RELIC_API_KEY="<user-api-key>"
export NEW_RELIC_ACCOUNT_ID="<account-id>"
export NEW_RELIC_REGION="US"
ACCOUNT_ID="$(aws sts get-caller-identity --query Account --output text)"
STATE_BUCKET="oficina-mecanica-terraform-state-${ACCOUNT_ID}"
ECR_IMAGE="$(terraform -chdir=infra/aws/platform output -raw ecr_repository_url):latest"

terraform -chdir=infra/aws/workloads init \
  -backend-config="bucket=${STATE_BUCKET}"

terraform -chdir=infra/aws/workloads apply \
  -var="state_bucket_name=${STATE_BUCKET}" \
  -var="api_image=${ECR_IMAGE}"
```

Para consultar a senha administrativa sem expo-la em arquivos:

```bash
terraform -chdir=infra/aws/workloads output -raw admin_password
```

Para conferir a integracao depois do deploy:

```bash
kubectl get pods,service -n kong
kubectl get ingress -n oficina-mecanica
kubectl get pods -n newrelic
kubectl get pods,service,hpa -n oficina-mecanica
terraform -chdir=infra/aws/workloads output -raw api_url
```

O Service `kong-gateway-proxy` deve apresentar um endereco externo. O Service
`oficina-mecanica-api` deve continuar como `ClusterIP` e sem endereco externo.

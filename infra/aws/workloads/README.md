# Workloads Kubernetes na AWS

Este stack e executado depois de `bootstrap` e `platform`. Ele le o estado da
plataforma no S3 e publica no EKS apenas os componentes da aplicacao:

- namespace;
- ConfigMap;
- Secret;
- Deployment da API;
- Service `LoadBalancer`;
- Horizontal Pod Autoscaler.

O MySQL nao e criado como pod neste ambiente. A conexao da API aponta para a
instancia RDS privada criada pelo stack `platform`. O Metrics Server tambem e
gerenciado pela plataforma como add-on do EKS.

As senhas do banco, do JWT e do administrador nao ficam gravadas no Git. Elas
ficam nos estados criptografados no S3 e no Secret do Kubernetes.

## Execucao manual para diagnostico

O fluxo normal e automatizado pelo GitHub Actions. Para executar manualmente,
com a plataforma ja criada:

```bash
export AWS_PROFILE=academy
STATE_BUCKET="$(terraform -chdir=infra/aws/bootstrap output -raw state_bucket_name)"
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

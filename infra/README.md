# Infraestrutura local com Terraform

Esta pasta contem os scripts Terraform da Fase 2.

A proposta aqui e provisionar a infraestrutura local sem depender de AWS, Azure ou GCP. O Docker Desktop continua sendo necessario como engine Docker, mas o cluster Kubernetes local e criado pelo Terraform usando kind.

## O que o Terraform cria

O Terraform cria e gerencia:

- cluster Kubernetes local com kind;
- carregamento da imagem `oficina-mecanica-api:local` dentro do node kind;
- namespace `oficina-mecanica`;
- ConfigMap da aplicacao;
- Secret com senhas e chave JWT;
- PVC do MySQL;
- Deployment e Service do MySQL;
- Metrics Server;
- Deployment e Service da API;
- HPA da API baseado em CPU e memoria.

## Como os arquivos estao organizados

```text
infra/
  versions.tf
  variables.tf
  main.tf
  outputs.tf
  manifests/
    00-namespaces/
    01-config/
    02-database-storage/
    03-database/
    04-observability-rbac/
    05-observability-service/
    06-observability-deployment/
    07-observability-apiservice/
    08-api/
    09-autoscaling/
```

Os diretorios numerados existem para deixar clara a ordem de criacao dos recursos.

## Pre-requisitos

Antes de aplicar a infraestrutura, garanta que voce tem:

- Docker Desktop instalado e rodando;
- Terraform instalado;
- imagem Docker da API criada localmente.

Nao e necessario habilitar o Kubernetes do Docker Desktop para este fluxo. O cluster local sera criado pelo Terraform usando kind.

## Como aplicar

Na raiz do projeto, gere a imagem da API:

```bash
docker build -t oficina-mecanica-api:local .
```

Entre na pasta de infraestrutura:

```bash
cd infra
```

Inicialize o Terraform:

```bash
terraform init
```

Esse comando baixa os providers usados pelo projeto:

- `tehcyx/kind`, para criar o cluster Kubernetes local;
- `gavinbunney/kubectl`, para aplicar os manifests YAML como resources do Terraform.

Veja o plano de execucao:

```bash
terraform plan
```

Esse comando mostra o que sera criado antes de alterar qualquer coisa.

Aplique a infraestrutura:

```bash
terraform apply
```

Esse comando cria o cluster kind, carrega a imagem local da API no node do cluster e aplica os manifests Kubernetes.

O carregamento da imagem e feito por um `terraform_data` com `local-exec`, usando Docker para copiar a imagem para dentro do node kind. Os recursos Kubernetes da aplicacao continuam sendo criados como resources Terraform do tipo `kubectl_manifest`.

Depois disso, a API ja fica disponivel em:

```text
http://localhost:18080/swagger/index.html
```

## Como validar

O Terraform grava o kubeconfig do cluster em:

```text
infra/.terraform/oficina-mecanica-kubeconfig
```

Para usar `kubectl` nesse cluster:

```bash
export KUBECONFIG="$(pwd)/.terraform/oficina-mecanica-kubeconfig"
```

Esse `export` nao e necessario para a aplicacao funcionar. Ele serve apenas para o `kubectl` saber qual cluster consultar.

Depois consulte os recursos:

```bash
kubectl get nodes
kubectl get pods -n oficina-mecanica
kubectl get service -n oficina-mecanica
kubectl get hpa -n oficina-mecanica
```

A API fica disponivel em:

```text
http://localhost:18080/swagger/index.html
```

## Como atualizar a imagem da API

Quando alterar codigo da aplicacao, gere novamente a imagem:

```bash
docker build -t oficina-mecanica-api:local ..
```

Depois force o Terraform a carregar a nova imagem dentro do node kind:

```bash
terraform apply -replace=terraform_data.load_api_image
```

Por fim, reinicie o deployment da API:

```bash
export KUBECONFIG="$(pwd)/.terraform/oficina-mecanica-kubeconfig"
kubectl rollout restart deployment/oficina-mecanica-api -n oficina-mecanica
```

## Como acessar o banco

```bash
kubectl exec -it deployment/mysql -n oficina-mecanica -- mysql -uoficina_user -poficina_pass oficina_mecanica
```

Dentro do MySQL:

```sql
SHOW TABLES;
```

## Como destruir

Para remover a infraestrutura criada pelo Terraform:

```bash
terraform destroy
```

Esse comando remove os manifests Kubernetes e o cluster kind criado localmente.

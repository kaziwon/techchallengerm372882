# Plataforma AWS com Terraform

Este diretorio provisiona os recursos principais da aplicacao na AWS Academy:

- VPC dedicada;
- duas sub-redes publicas para EKS e Load Balancer;
- duas sub-redes privadas para RDS;
- Internet Gateway, sem NAT Gateway;
- repositorio privado Amazon ECR;
- cluster Amazon EKS e um Managed Node Group;
- Metrics Server como add-on do EKS para alimentar o HPA;
- instancia Amazon RDS MySQL privada.

As roles IAM exigidas pelo EKS nao sao criadas pelo Terraform. O codigo localiza
automaticamente as roles `LabEksClusterRole` e `LabEksNodeRole` fornecidas pela
AWS Academy.

## Estado remoto

O estado desta plataforma fica no bucket preparado pelo script de bootstrap.
Para inicializar localmente:

```bash
export AWS_PROFILE=academy
./infra/aws/scripts/bootstrap-state.sh
ACCOUNT_ID="$(aws sts get-caller-identity --query Account --output text)"
STATE_BUCKET="oficina-mecanica-terraform-state-${ACCOUNT_ID}"

terraform -chdir=infra/aws/platform init \
  -backend-config="bucket=${STATE_BUCKET}"
```

O backend utiliza criptografia e lock file no S3. Nenhuma credencial e gravada
nos arquivos Terraform.

## Validar e planejar

```bash
export AWS_PROFILE=academy
terraform -chdir=infra/aws/platform fmt -check
terraform -chdir=infra/aws/platform validate
terraform -chdir=infra/aws/platform plan
```

O `plan` consulta a conta e mostra os recursos antes da criacao. Somente o
`apply` cria recursos e inicia cobrancas na AWS.

## Destruir

Destrua primeiro a plataforma e somente depois o bucket de estado:

```bash
export AWS_PROFILE=academy
terraform -chdir=infra/aws/platform destroy
./infra/aws/scripts/destroy-state.sh
```

Na pipeline final, o bucket, o backend e a plataforma serao preparados
automaticamente antes da publicacao da aplicacao.

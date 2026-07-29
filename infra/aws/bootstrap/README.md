# Bootstrap Terraform na AWS

Este diretorio cria o bucket S3 usado para armazenar o estado remoto da
infraestrutura AWS. Ele e separado porque o bucket precisa existir antes que o
restante do Terraform possa utiliza-lo como backend.

O bucket possui:

- nome unico baseado no ID da conta AWS;
- bloqueio completo de acesso publico;
- criptografia no servidor com AES-256;
- versionamento para recuperacao de estados anteriores;
- politica que recusa conexoes sem TLS.

## Validar sem criar recursos

Com uma sessao ativa da AWS Academy e o perfil local configurado:

```bash
export AWS_PROFILE=academy
terraform -chdir=infra/aws/bootstrap init
terraform -chdir=infra/aws/bootstrap validate
terraform -chdir=infra/aws/bootstrap plan
```

Esses comandos inicializam e validam o Terraform e exibem o que seria criado.
Eles nao criam recursos na AWS.

## Criar o bucket

Depois de revisar o plano:

```bash
export AWS_PROFILE=academy
terraform -chdir=infra/aws/bootstrap apply
```

O estado deste bootstrap permanece local e nao deve ser versionado. Os estados
dos demais componentes AWS serao armazenados no bucket criado aqui.

## Remover o bucket

O bucket deve ser removido somente depois que todos os outros estados e recursos
AWS tiverem sido destruidos:

```bash
export AWS_PROFILE=academy
terraform -chdir=infra/aws/bootstrap destroy
```

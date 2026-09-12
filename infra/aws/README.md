# Infraestrutura AWS

A infraestrutura foi dividida em um bootstrap tecnico e dois stacks Terraform
para respeitar a ordem em que os recursos passam a existir:

1. `scripts/bootstrap-state.sh`: cria ou recupera o bucket S3 usado como backend;
2. `platform`: cria VPC, ECR, EKS, Metrics Server e RDS;
3. `workloads`: conecta ao EKS, publica a API atras do Kong Gateway e configura a observabilidade no New Relic.

Essa separacao resolve o problema do primeiro deploy: o Terraform nao pode usar
como backend um bucket que ainda nao existe. O script usa o AWS CLI de forma
idempotente: cria e configura o bucket quando necessario ou reutiliza o existente.

O Learner Lab aplica uma Service Control Policy que nega
`s3:GetBucketObjectLockConfiguration`. Como o recurso `aws_s3_bucket` consulta
essa API mesmo sem Object Lock configurado, o bucket de backend e a unica excecao
ao provider AWS. EKS, RDS, VPC, ECR, Kubernetes, Kong e New Relic permanecem
declarados e gerenciados pelo Terraform.

## Execucao pela pipeline

O caminho de producao e a execucao manual de
`.github/workflows/entrega-continua.yml` a partir da branch `main`. A pipeline:

1. autentica com as credenciais temporarias guardadas nos GitHub Secrets;
2. executa `scripts/bootstrap-state.sh`;
3. executa `plan` e `apply` de `platform`;
4. gera e publica a imagem no ECR;
5. executa `plan` e `apply` de `workloads`;
6. instala o Kong e deixa a API acessivel apenas por seu Service interno;
7. instala a integracao Kubernetes do New Relic e configura APM, uptime e alertas;
8. valida o Kong, o rollout, o HPA, as metricas e os endpoints publicos.

Pull requests e commits executam somente a integracao continua. Eles nao
provisionam recursos na AWS automaticamente.

Os estados de `platform` e `workloads` ficam em chaves diferentes no mesmo
bucket S3, com versionamento, criptografia e lock habilitados.

## Execucao depois de Reset

Se o AWS Academy apagar toda a conta, o bucket deixa de existir. Na execucao
seguinte, o bootstrap detecta essa ausencia e cria um bucket novo. Como os
estados remotos tambem foram apagados, os outros dois stacks reconhecem uma
infraestrutura vazia e criam tudo novamente.

As credenciais temporarias nao podem ser renovadas pelo projeto. Depois de
iniciar uma nova sessao do Learner Lab, atualize `AWS_ACCESS_KEY_ID`,
`AWS_SECRET_ACCESS_KEY` e `AWS_SESSION_TOKEN` nos GitHub Secrets.

Os recursos externos do New Relic nao sao apagados pelo Reset do AWS Academy.
Antes de usar Reset, prefira executar o workflow `Destruir infraestrutura AWS`,
que remove do New Relic o monitor e as condicoes gerenciadas pelo Terraform antes
de apagar o estado no S3.

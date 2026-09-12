# Deploy AWS da aplicacao

Este diretorio contem apenas o Terraform pertencente a aplicacao executada no
EKS. A infraestrutura da Fase 3 esta dividida entre quatro repositorios:

1. `oficina-mecanica-infra-banco`: S3 de state, VPC e RDS;
2. `oficina-mecanica-infra-kubernetes`: EKS, ECR, Metrics Server, Kong e agente
   de infraestrutura do New Relic;
3. `techchallengerm372882`: codigo, migrations, imagem, Deployment, Service,
   HPA, rotas da API no Kong e observabilidade da aplicacao;
4. `oficina-mecanica-autenticacao-lambda`: autenticacao por CPF e emissao de JWT.

## Execucao pela pipeline

O workflow `Entrega continua AWS - Aplicacao` e manual e deve ser executado na
`main` depois das entregas de banco e Kubernetes. Ele:

1. confirma a existencia dos states anteriores;
2. le o endereco do ECR e o nome do EKS;
3. cria e publica a imagem Docker da API;
4. aplica `infra/aws/application`;
5. valida rollout, HPA, healthcheck, Swagger e autenticacao JWT pelo Kong.

O state desta raiz usa a chave
`oficina-mecanica/application/terraform.tfstate` no bucket S3 compartilhado.

## Destruicao

Execute primeiro a destruicao da Lambda. Depois, o workflow
`Destruir infraestrutura AWS - Aplicacao` remove somente os recursos da
aplicacao e do New Relic que este repositorio gerencia. Kubernetes e banco sao
destruidos pelas pipelines dos seus proprios repositorios.

As credenciais do AWS Academy sao temporarias. Ao iniciar uma nova sessao,
atualize `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY` e `AWS_SESSION_TOKEN` nos
GitHub Secrets de cada repositorio.

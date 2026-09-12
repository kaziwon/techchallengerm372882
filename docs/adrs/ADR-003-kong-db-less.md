# ADR-003: Kong Gateway em modo DB-less

- Status: aceita
- Data: 2026-09-12

## Contexto

A Fase 3 exige um API Gateway. A equipe estudou Kong com interface grafica, mas
cadastrar rotas manualmente impediria a reproducao integral do ambiente por
Terraform.

## Decisao

Instalar o chart oficial do Kong no EKS em modo DB-less e usar o Kong Ingress
Controller. Services, Ingresses, plugins, consumers e credenciais JWT sao
declarados por Terraform e Helm.

O Service proxy do Kong usa `LoadBalancer` e cria o Network Load Balancer. O
Service da API usa `ClusterIP`, portanto todo acesso externo passa pelo gateway.

As rotas sao divididas em:

- publicas: healthcheck, Swagger, login administrativo, consulta de OS por
  CPF/CNPJ e notificacao externa de orcamento;
- protegidas: cadastros e operacoes administrativas de clientes, veiculos,
  servicos, pecas e ordens de servico.

O plugin JWT rejeita chamadas sem credencial valida antes que elas cheguem ao
pod. A API tambem valida o token como defesa adicional.

## Motivos

- toda configuracao fica versionada e reaplicavel;
- nenhuma operacao manual na interface do Kong e necessaria;
- Ingresses permitem declarar grupos de rotas sem cadastrar endpoint por
  endpoint;
- o mesmo JWT emitido pela Lambda pode ser validado localmente;
- o gateway pode ser demonstrado pelos headers de resposta e pelo teste 401.

## Alternativas rejeitadas

- Kong Manager com banco: adicionaria outro banco e configuracao manual;
- AWS API Gateway alem do Kong: duplicaria gateways, rotas e custos;
- expor o Service da API diretamente: permitiria contornar autenticacao e
  politicas do gateway;
- autenticar somente dentro da API: nao atenderia ao objetivo de protecao no
  API Gateway.

## Consequencias

- mudancas nas rotas exigem novo `terraform apply` da aplicacao;
- a disponibilidade publica depende do Kong e do NLB;
- configuracoes incorretas de regex podem ser rejeitadas pelo admission webhook;
- o chart e os recursos declarativos substituem a interface grafica como fonte
  de verdade.

# ADR-001: Comunicacao entre componentes

- Status: aceita
- Data: 2026-09-12

## Contexto

A solucao possui clientes externos, Kong, API, Lambda, RDS e New Relic. Era
necessario definir como esses componentes se comunicam sem criar dependencias
circulares ou obrigar a API a conhecer detalhes da funcao serverless.

## Decisao

Adotar os seguintes estilos de comunicacao:

| Origem e destino | Estilo | Contrato |
|---|---|---|
| Cliente -> Lambda | sincrono | HTTPS e JSON pela Function URL |
| Cliente -> Kong -> API | sincrono | HTTP/JSON e Bearer JWT |
| API e Lambda -> RDS | sincrono | protocolo MySQL com TLS |
| API -> New Relic | assincrono | agente APM, logs e traces |
| EKS -> New Relic | assincrono | agente de infraestrutura |
| Terraform entre repositorios | somente no deploy | outputs por remote state no S3 |

A API nao chama a Lambda durante uma requisicao protegida. O JWT funciona como
credencial portavel: a Lambda assina e Kong/API validam localmente.

Dentro da aplicacao, o fluxo respeita a regra de dependencia da Clean
Architecture:

```text
Controller HTTP -> Controller Clean -> Use Case -> Interface de Gateway
                                              <- Gateway/Source externo
```

## Motivos

- HTTP/JSON e adequado aos consumidores e facilmente demonstravel no Swagger e
  Postman;
- validacao local do JWT evita indisponibilidade em cascata entre API e Lambda;
- acesso MySQL direto e suficiente para o unico banco do MVP;
- telemetria assincrona nao bloqueia a resposta da API;
- remote state compartilha somente contratos de infraestrutura no momento do
  deploy, nao em tempo de execucao.

## Alternativas rejeitadas

- mensageria para todas as operacoes: aumentaria a complexidade e mudaria a
  semantica sincrona dos endpoints existentes;
- chamada API -> Lambda para validar cada token: criaria latencia e acoplamento;
- um unico state Terraform: reduziria a autonomia dos quatro repositorios;
- acesso publico ao banco: simplificaria conectividade, mas violaria o limite de
  seguranca definido para o RDS.

## Consequencias

- clientes precisam obter e enviar o JWT;
- indisponibilidade do RDS afeta API e autenticacao, sendo observada por erros e
  healthchecks;
- alteracoes nos nomes dos outputs exigem compatibilidade entre repositorios;
- a correlacao usa `X-Correlation-ID`, `traceId` e `spanId` para ligar requisicao,
  log e trace no New Relic.

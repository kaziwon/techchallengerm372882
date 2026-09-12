# Checklist da entrega final

Esta checklist concentra o que deve ser feito depois que codigo e documentacao
estiverem revisados. Ate essa etapa, nenhuma infraestrutura AWS precisa ficar
ativa.

## 1. Preparar os quatro repositorios

- Integrar as alteracoes por Pull Request nas quatro branches `main`.
- Proteger `main` contra push direto, force push e exclusao.
- Exigir Pull Request para merge.
- Adicionar o usuario `soat-architecture` como colaborador nos quatro
  repositorios.
- Confirmar que as quatro execucoes de integracao continua terminaram verdes.

## 2. Renovar as credenciais

Inicie o AWS Academy Learner Lab e atualize, nos quatro repositorios:

- secret `AWS_ACCESS_KEY_ID`;
- secret `AWS_SECRET_ACCESS_KEY`;
- secret `AWS_SESSION_TOKEN`.

No repositorio Kubernetes, confirme o secret `NEW_RELIC_LICENSE_KEY`.

No repositorio da aplicacao, confirme:

- secret `NEW_RELIC_LICENSE_KEY`;
- secret `NEW_RELIC_USER_API_KEY`;
- variable `NEW_RELIC_ACCOUNT_ID`.

Nunca coloque esses valores em commits, logs, README ou no video.

## 3. Executar uma unica rodada de deploy

Dispare manualmente os workflows pela branch `main` nesta ordem:

1. `Entrega continua AWS - Banco`;
2. `Entrega continua AWS - Kubernetes`;
3. `Entrega continua AWS - Aplicacao`;
4. `Entrega continua AWS - Lambda`.

Cada pipeline valida seus states dependentes. Espere uma terminar antes de
iniciar a seguinte. Guarde os Summaries, pois eles mostram endpoint do Kong,
Swagger, Function URL, recursos Kubernetes e links do New Relic.

## 4. Validar os requisitos funcionais

1. Abra o Swagger pelo endpoint do Kong.
2. Confirme que `/health` responde com HTTP 200 e header `X-Correlation-ID`.
3. Confirme que uma rota protegida responde HTTP 401 sem token.
4. Faça o login administrativo e crie um cliente ativo com CPF valido.
5. Chame a Function URL com esse CPF e guarde o JWT retornado.
6. Use o JWT da Lambda em uma rota protegida e confirme uma resposta diferente
   de 401.
7. Crie uma ordem de servico e percorra Diagnostico, Aprovacao, Execucao,
   Finalizacao e Entrega.
8. Teste a notificacao externa de aprovacao ou recusa do orcamento.

A collection em `docs/postman/oficina-mecanica.postman_collection.json` ja
salva os tokens e IDs usados nesse fluxo.

## 5. Validar observabilidade e escalabilidade

- Mostre latencia media e p95 da API no dashboard.
- Mostre CPU e memoria dos pods no EKS.
- Mostre o monitor sintetico de `/health` e seu uptime.
- Localize os eventos `OrdemServicoCriada` e
  `OrdemServicoStatusAlterado` nos logs.
- Pesquise um `correlationId` conhecido e abra o trace relacionado.
- Mostre volume diario de OS e tempo medio de Diagnostico, Execucao e
  Finalizacao.
- Mostre a condicao e o historico do alerta de falhas em OS.
- Gere carga controlada e registre o HPA aumentando a quantidade de replicas.

O roteiro detalhado para o New Relic esta em
[`observabilidade.md`](observabilidade.md).

## 6. Gravar o video

O video deve ter no maximo 15 minutos e demonstrar:

- autenticacao por CPF na Lambda;
- pipelines de CI/CD e deploy automatizado apos o disparo manual;
- acesso negado sem JWT e permitido com JWT;
- consumo das APIs pelo Kong;
- dashboard com analise ao vivo;
- logs, `correlationId` e traces;
- recursos do EKS e atuacao do HPA.

As pipelines podem ser mostradas ja concluidas, conforme a orientacao da aula.

## 7. Destruir sem perder dependencias

Depois da gravacao, execute os workflows de destruicao na ordem inversa:

1. Lambda;
2. aplicacao;
3. Kubernetes;
4. banco.

Essa ordem remove primeiro os recursos que dependem dos states e da rede. O
banco remove por ultimo o bucket S3 compartilhado.

## 8. Montar o PDF de entrega

O documento unico enviado ao Portal do Aluno deve conter:

- links dos quatro repositorios;
- link publico ou nao listado do video;
- links da documentacao arquitetural;
- confirmacao de que `soat-architecture` foi adicionado aos quatro repositorios.

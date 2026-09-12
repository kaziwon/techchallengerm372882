# Monitoramento e observabilidade

## Visao geral

A observabilidade usa New Relic em duas frentes:

- o agente .NET acompanha transacoes, latencia, erros, traces e logs da API;
- o chart `nri-bundle` acompanha CPU e memoria dos recursos no EKS.

O Terraform da aplicacao tambem cria o monitor sintetico, as condicoes de alerta
e o dashboard `Oficina Mecanica - Operacao AWS`. A configuracao esta em
`infra/aws/application/observability.tf`.

Nenhum agente precisa ficar ativo durante o desenvolvimento. A imagem possui o
agente, mas `CORECLR_ENABLE_PROFILING` fica desabilitado por padrao. O ConfigMap
da AWS habilita o profiler no pod de producao. No Docker Compose local, a flag
`NEW_RELIC_ENABLED` do arquivo `.env` controla esse comportamento.

## Requisitos atendidos

| Requisito | Fonte no New Relic | Implementacao |
|---|---|---|
| Latencia das APIs | `Transaction` | APM .NET e painel de media/p95 |
| CPU e memoria do Kubernetes | `K8sContainerSample` | `nri-bundle`, Metrics Server e paineis por pod |
| Healthcheck e uptime | `SyntheticCheck` | chamada externa a `/health` a cada minuto |
| Alerta de falhas em OS | `Transaction` | condicao NRQL para HTTP 5xx em `OrdensServico` |
| Logs JSON | `Log` | formatador JSON do ASP.NET e forwarding do agente |
| Correlacao | `correlationId`, `trace.id`, `span.id` | middleware e distributed tracing |
| Volume diario de OS | evento `OrdemServicoCriada` | painel por dia |
| Tempo medio por status | evento `OrdemServicoStatusAlterado` | media da duracao anterior por etapa |
| Erros de integracao | `TransactionError` e `Log` | tabela de falhas e logs correlacionados |

## Logs estruturados e correlacao

O formatador de console da aplicacao gera uma linha JSON por evento. O
middleware aceita o header `X-Correlation-ID`; quando ele nao e enviado, usa o
Trace ID corrente ou gera um identificador. O mesmo valor:

1. e devolvido no header da resposta;
2. entra no escopo de todos os logs daquela requisicao como `correlationId`;
3. aparece junto de `traceId` e `spanId` quando a requisicao esta instrumentada.

Exemplo de chamada para demonstracao:

```bash
curl -i \
  -H 'X-Correlation-ID: video-fase3-os-001' \
  http://ENDERECO_DO_KONG/health
```

No New Relic, abra `Logs` e procure por:

```text
correlationId:"video-fase3-os-001"
```

Ao abrir o registro, os campos `trace.id` e `span.id` permitem navegar do log
para o trace da requisicao. Essa e a correlacao: o mesmo atendimento pode ser
seguido entre a chamada HTTP, os logs e o trace distribuido.

## Eventos de ordem de servico

O controller HTTP emite atributos estruturados sem introduzir framework nas
camadas internas da Clean Architecture.

### OrdemServicoCriada

Campos principais:

- `evento = OrdemServicoCriada`;
- `ordemServicoId`;
- `clienteId`;
- `veiculoId`;
- `statusAtual`.

### OrdemServicoStatusAlterado

Campos principais:

- `evento = OrdemServicoStatusAlterado`;
- `ordemServicoId`;
- `statusAnterior`;
- `statusAtual`;
- `duracaoStatusSegundos`.

O tempo e calculado entre os timestamps reais da OS. Quando um orcamento e
recusado, `DiagnosticoEm` recebe o instante de retorno ao diagnostico. Assim, um
novo envio de orcamento mede somente o retrabalho e nao inclui o periodo anterior
de espera pela aprovacao.

O dashboard agrupa:

- `EmDiagnostico` como Diagnostico;
- `EmExecucao` como Execucao;
- `Finalizada` como Finalizacao, ate a entrega.

## Dashboard

O dashboard possui duas paginas.

### Negocio

- volume diario de ordens de servico;
- tempo medio em minutos por Diagnostico, Execucao e Finalizacao;
- erros e falhas nas integracoes agrupados por transacao e classe.

### Operacao

- latencia media e percentil 95 da API;
- taxa de respostas HTTP 5xx;
- uptime do `/health`;
- CPU por pod;
- memoria por pod;
- logs de erro com dados de correlacao.

O link direto e publicado no Summary do workflow de entrega da aplicacao.

## Alertas

As condicoes usam a policy existente `Oficina Mecanica - Monitoramento`, que
possui o workflow de notificacao por e-mail.

| Condicao | Disparo | Janela |
|---|---|---|
| Falhas no processamento de ordens de servico - AWS | ao menos um HTTP 5xx em rota de OS | 1 minuto |
| Indisponibilidade do healthcheck - AWS | ao menos uma verificacao sintetica com falha | 1 minuto |

## Protocolo da demonstracao final

Execute este roteiro somente depois do deploy final dos quatro repositorios.

1. Abra o Summary da entrega da aplicacao e copie o endpoint do Kong e o link do
   dashboard.
2. Abra `/swagger/index.html` pelo Kong e gere algumas chamadas para preencher
   latencia e throughput.
3. Crie uma OS e avance o fluxo completo. Espere alguns segundos entre iniciar
   diagnostico e enviar orcamento, entre aprovar e finalizar, e entre finalizar
   e entregar.
4. Atualize a pagina Negocio e mostre volume diario e medias por etapa.
5. Envie uma chamada com `X-Correlation-ID` conhecido, localize esse valor em
   Logs e abra o trace relacionado.
6. Mostre a pagina Operacao com latencia, uptime, CPU e memoria dos pods.
7. Mostre em Alerts a condicao de falha de OS e seu workflow de e-mail.
8. Em Kubernetes, compare a quantidade de pods e os graficos de CPU/memoria
   durante uma carga controlada para comprovar o HPA.

Para demonstrar o alerta sem esperar durante a gravacao, gere a falha controlada
antes do video e aguarde a recuperacao do ambiente. O historico do incidente
permanece no New Relic e pode ser apresentado junto da condicao e da notificacao.

## Limpeza de emergencia

O `terraform destroy` da aplicacao remove dashboard, monitor e condicoes. Se um
Reset do AWS Academy apagar o state antes da destruicao, use:

```bash
NEW_RELIC_ACCOUNT_ID=SEU_ACCOUNT_ID \
  ./infra/aws/scripts/cleanup-new-relic-aws.sh
```

O script solicita a User API Key sem exibi-la e preserva a policy e o workflow
de notificacao compartilhados.

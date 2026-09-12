# ADR-002: Escalabilidade horizontal da API

- Status: aceita
- Data: 2026-09-12

## Contexto

O enunciado exige escalabilidade automatica por CPU e memoria no Kubernetes. A
API e stateless: autenticacao usa JWT e os dados persistentes ficam no RDS, o
que permite executar varias replicas equivalentes.

## Decisao

Usar `HorizontalPodAutoscaler` com `autoscaling/v2` sobre o Deployment da API:

- minimo de 1 pod;
- maximo de 3 pods no ambiente academico;
- alvo medio de CPU: 70%;
- alvo medio de memoria: 80%;
- requests por pod: 100m CPU e 256Mi de memoria;
- limits por pod: 500m CPU e 512Mi de memoria.

O Metrics Server e instalado como add-on gerenciado do EKS. O Deployment usa
readiness e liveness probes em `/health`, garantindo que apenas replicas prontas
recebam trafego.

O Managed Node Group inicia com um node `t3.medium` e permite ate dois nodes. O
HPA escala pods, nao maquinas. A faixa do node group reserva uma evolucao para
Cluster Autoscaler ou ajuste manual, que nao faz parte do escopo atual.

## Motivos

- CPU e memoria cobrem tanto carga de processamento quanto pressao de memoria;
- `autoscaling/v2` permite combinar as duas metricas;
- uma replica reduz custo quando o ambiente esta ocioso;
- tres replicas sao suficientes para demonstracao sem exceder rapidamente os
  recursos do Learner Lab;
- probes evitam que o Load Balancer envie chamadas durante inicializacao ou
  falha do processo.

## Alternativas rejeitadas

- Vertical Pod Autoscaler: altera recursos do pod e pode exigir reinicios; nao
  demonstra replicas horizontais;
- numero fixo de replicas: nao atende ao requisito de escalabilidade automatica;
- maximo de dez pods, usado em teste local: desproporcional ao unico node
  academico inicial e sujeito a pods pendentes por falta de capacidade;
- escalabilidade baseada somente em CPU: nao reagiria a consumo excessivo de
  memoria.

## Consequencias

- o HPA depende de requests de recursos e do Metrics Server saudavel;
- picos curtos podem terminar antes que novas replicas fiquem prontas;
- o RDS permanece compartilhado entre replicas e deve suportar as conexoes;
- a demonstracao deve exibir `kubectl get hpa` e `kubectl get pods -w` durante
  uma carga controlada;
- CPU e memoria dos pods ficam visiveis no dashboard do New Relic.

# RFC-001: Plataforma de producao na AWS

- Status: aceita e implementada
- Data: 2026-09-12
- Autores: grupo do Tech Challenge

## Contexto

A Fase 3 exige infraestrutura em nuvem, API Gateway, computacao serverless,
banco gerenciado, Kubernetes escalavel, infraestrutura como codigo e pipelines
independentes. O ambiente disponivel e o AWS Academy Learner Lab, com creditos,
credenciais temporarias e restricoes de permissoes.

## Objetivos

- entregar um unico ambiente de producao reproduzivel;
- criar e remover recursos sem configuracao manual na console AWS;
- manter responsabilidades separadas em quatro repositorios;
- permitir demonstracao de gateway, Lambda, RDS, EKS, HPA e observabilidade;
- reduzir consumo de creditos quando o trabalho nao estiver sendo demonstrado.

## Proposta

A plataforma usa:

- Amazon VPC com duas sub-redes publicas e duas privadas;
- Amazon RDS for MySQL nas sub-redes privadas;
- Amazon EKS com Managed Node Group nas sub-redes publicas;
- Amazon ECR para a imagem da API;
- Kong Gateway no EKS, exposto por Network Load Balancer;
- AWS Lambda com Function URL para autenticacao por CPF;
- Amazon S3 para states remotos do Terraform;
- CloudWatch Logs para a Lambda;
- New Relic SaaS para APM, logs, Kubernetes, uptime, alertas e dashboard.

Os recursos sao divididos nos repositorios de banco, Kubernetes, aplicacao e
Lambda. Cada repositorio possui seu proprio state e sua propria CI/CD. Outputs
nao secretos sao consumidos por `terraform_remote_state`; valores sensiveis
permanecem marcados como `sensitive` e nao sao impressos nas pipelines.

O bucket usado como backend precisa existir antes de o Terraform inicializar.
No AWS Academy, a politica da organizacao tambem bloqueia a consulta de Object
Lock feita pelo resource de bucket. Por isso, apenas esse bootstrap e executado
por um script AWS CLI, com acesso publico bloqueado, versionamento e criptografia
habilitados. A infraestrutura funcional exigida pelo trabalho permanece
provisionada pelo Terraform.

## Estrategia de entrega

A integracao continua roda em pull requests e em `main`, sem provisionar
recursos. A entrega de producao usa `workflow_dispatch` e deve ser iniciada
manualmente na seguinte ordem:

1. banco;
2. Kubernetes;
3. aplicacao;
4. Lambda.

A destruicao usa a ordem inversa. Os workflows validam os states dependentes
para impedir a remocao fora de ordem.

O acionamento manual foi escolhido porque cada sessao da AWS Academy possui
credenciais temporarias e a criacao automatica a cada merge consumiria creditos
sem necessidade. O deploy continua automatizado depois do clique: Terraform e
os testes de smoke executam sem intervencao adicional.

## Seguranca

- nenhuma credencial AWS e versionada;
- os tres valores temporarios do Learner Lab ficam em GitHub Secrets;
- o RDS nao possui endpoint publico;
- a API nao e exposta diretamente, somente pelo Kong;
- Security Groups referenciam outros Security Groups em vez de liberar CIDRs
  amplos para o MySQL;
- o JWT e validado no Kong e novamente na API;
- o CPF completo, senhas e tokens nao sao enviados aos logs.

## Alternativas avaliadas

### Infraestrutura local com kind

Foi usada na Fase 2, mas nao atende ao objetivo da Fase 3 de demonstrar servicos
gerenciados em nuvem.

### Amazon ECS ou containers sem Kubernetes

Reduziria a complexidade operacional, mas nao atenderia ao requisito explicito
de Kubernetes e HPA.

### Deploy automatico em todo merge

E comum em contas permanentes, mas foi rejeitado neste ambiente por causa das
credenciais temporarias e do custo do EKS e RDS.

## Consequencias

### Positivas

- ambiente reproduzivel a partir de uma conta vazia;
- separacao clara de ownership e pipelines;
- demonstracao direta dos servicos exigidos;
- recursos podem ser destruidos quando nao estiverem em uso.

### Negativas

- a ordem dos quatro deploys precisa ser respeitada;
- EKS e RDS levam varios minutos para criar e destruir;
- secrets do Learner Lab precisam ser renovados a cada sessao;
- o ambiente academico usa configuracoes de baixo custo, sem alta
  disponibilidade Multi-AZ.

## Criterios de aceite

- os quatro workflows de CI concluem com sucesso;
- os quatro deploys partem de `main` e concluem na ordem documentada;
- o Swagger responde pelo endpoint do Kong;
- uma rota protegida devolve 401 sem JWT e sucesso com JWT valido;
- a Lambda consulta o RDS e emite JWT para cliente ativo;
- recursos e dashboards aparecem no EKS e no New Relic.

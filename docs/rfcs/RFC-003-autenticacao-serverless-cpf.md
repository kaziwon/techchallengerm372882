# RFC-003: Autenticacao serverless por CPF

- Status: aceita e implementada
- Data: 2026-09-12
- Autores: grupo do Tech Challenge

## Contexto

A Fase 3 exige proteger rotas sensiveis com autenticacao via CPF e implementar
ao menos uma funcao serverless que valide o CPF, consulte o status do cliente ou
gere o token. A solucao escolheu concentrar as tres operacoes em uma unica
funcao especializada.

## Proposta

Criar uma AWS Lambda .NET independente da API principal, com Function URL HTTPS
e contrato `POST /`:

```json
{
  "cpf": "52998224725"
}
```

O fluxo da funcao e:

1. validar formato e digitos verificadores do CPF;
2. normalizar o documento;
3. consultar o cliente no RDS;
4. negar acesso se o cliente nao existir ou estiver inativo;
5. emitir JWT HMAC para o cliente ativo;
6. retornar identificacao do cliente, token e expiracao.

## Arquitetura interna

A Lambda tambem segue Clean Architecture:

- Domain: entidade `Cliente`;
- Application: `AutenticarClienteUseCase` e interfaces de gateway;
- Infrastructure: MySQL, validacao de CPF e assinatura HMAC;
- Function: adaptador do evento HTTP e composicao das dependencias.

As camadas internas nao dependem de AWS Lambda, MySQL ou bibliotecas de JWT.

## Integracao com o gateway

A Lambda e o Kong nao se chamam diretamente. O cliente recebe o JWT da Lambda e
o apresenta ao Kong na requisicao seguinte. O Kong valida o token localmente e
encaminha a chamada para a API, que faz uma segunda validacao.

O segredo, issuer e audience sao compartilhados por outputs sensiveis do state
da aplicacao. Isso garante compatibilidade sem expor os valores no codigo ou no
resumo da pipeline.

## Seguranca e privacidade

- a resposta usa `Cache-Control: no-store`;
- somente o metodo POST e aceito;
- CPF completo e JWT nunca sao gravados em logs;
- o CPF e mascarado nos eventos do CloudWatch;
- a Lambda fica nas sub-redes privadas para acessar o RDS;
- o Security Group do banco aceita a Lambda por identidade de Security Group;
- tokens possuem expiracao curta e validam issuer e audience.

Em um ambiente comercial, a Function URL deve receber protecoes adicionais,
como AWS WAF, rate limiting e dominio proprio. Para o escopo academico, as
respostas minimas e a validacao de cliente reduzem a exposicao.

## Alternativas avaliadas

### Autenticacao dentro da API principal

Seria mais simples, mas nao demonstraria computacao serverless e manteria a
autenticacao de cliente acoplada ao ciclo de vida do pod.

### Lambda Authorizer em API Gateway da AWS

E uma opcao valida, mas a arquitetura ja usa Kong como gateway exigido. Manter
dois gateways aumentaria custo e complexidade de configuracao.

### Tres funcoes pequenas

Separar validacao, consulta e token criaria chamadas adicionais e mais pontos de
falha para um fluxo curto e coeso. Uma funcao atende aos tres itens do requisito.

## Consequencias

### Positivas

- escala sob demanda e nao exige container permanentemente ativo;
- responsabilidade de autenticacao isolada em repositorio proprio;
- atende aos tres exemplos serverless do enunciado;
- testes unitarios nao dependem de AWS ou MySQL.

### Negativas

- o primeiro acesso pode sofrer cold start;
- o deploy depende dos states do banco e da aplicacao;
- a Function URL publica exige cuidados contra abuso em um ambiente real.

## Criterios de aceite

- CPF invalido retorna 400;
- cliente inexistente retorna 404;
- cliente inativo retorna 403;
- cliente ativo recebe JWT valido;
- o JWT permite acesso a uma rota protegida no Kong;
- logs estruturados registram resultado e `requestId` sem dados sensiveis.

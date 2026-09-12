# RFC-002: Banco relacional gerenciado

- Status: aceita e implementada
- Data: 2026-09-12
- Autores: grupo do Tech Challenge

## Contexto

O sistema persiste clientes, veiculos, catalogos, estoque, ordens de servico e
itens de orcamento. Esses dados possuem cardinalidades conhecidas, unicidade de
documento, placa e nomes de catalogo, alem de regras que relacionam uma OS ao
cliente e ao veiculo corretos.

A Fase 3 exige um banco em servico gerenciado e uma justificativa formal da
tecnologia escolhida.

## Decisao proposta

Usar Amazon RDS for MySQL 8.4, provisionado pelo Terraform em sub-redes privadas.
A aplicacao acessa o banco por Entity Framework Core com o provider Pomelo. A
Lambda usa MySqlConnector e executa apenas a consulta necessaria para
autenticacao.

## Justificativa

O modelo relacional oferece:

- chaves estrangeiras para evitar veiculos e ordens orfaos;
- indices unicos para CPF/CNPJ, placa e nomes de catalogo;
- consultas eficientes para localizar ordens por cliente e status;
- semantica transacional adequada a alteracoes de estoque e orcamento;
- migrations versionadas junto ao codigo que conhece o modelo.

O RDS atende ao requisito de servico gerenciado e remove do projeto a
responsabilidade de instalar, atualizar e manter o processo MySQL dentro do
cluster Kubernetes.

## Topologia

- `publicly_accessible` desabilitado;
- DB Subnet Group composto pelas duas sub-redes privadas;
- senha aleatoria criada pelo Terraform e marcada como sensivel;
- entrada TCP 3306 somente a partir dos Security Groups do EKS e da Lambda;
- conexao da aplicacao com `SslMode=Required`.

## Ownership do schema

As migrations ficam no repositorio da aplicacao principal. O repositorio de
banco cria a capacidade de persistencia, mas nao conhece tabelas de negocio.
Essa divisao permite evoluir o schema junto das entidades e dos casos de uso,
sem acoplar a infraestrutura AWS ao framework de persistencia.

## Alternativas avaliadas

### MySQL como pod no EKS

Foi usado localmente na Fase 2, mas exigiria cuidar de volume, backup,
recuperacao e disponibilidade dentro do cluster. Tambem nao seria um banco
gerenciado.

### DynamoDB

E gerenciado e serverless, mas exigiria remodelar os relacionamentos e padroes
de consulta. Para o dominio atual, isso aumentaria a complexidade sem beneficio
academico ou operacional proporcional.

### Amazon Aurora

Oferece mais recursos de disponibilidade e escalabilidade, mas possui custo e
complexidade maiores que o necessario para o MVP no AWS Academy.

## Consequencias

### Positivas

- integridade referencial e modelo aderente ao dominio;
- acesso compativel com a aplicacao e a Lambda;
- operacao do motor delegada ao servico gerenciado;
- banco isolado da internet.

### Negativas

- uma unica instancia academica e um ponto unico de falha;
- o inicio do ambiente depende do tempo de provisionamento do RDS;
- alteracoes incompativeis de schema exigem planejamento das migrations.

## Criterios de aceite

- RDS criado pelo Terraform e sem acesso publico;
- API aplica migrations e realiza operacoes CRUD;
- Lambda encontra cliente pelo CPF e le o campo `Ativo`;
- testes automatizados usam doubles ou banco em memoria, sem depender da AWS;
- diagrama ER e relacionamentos documentados.

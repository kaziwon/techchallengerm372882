# Tech Challenge - Fase 1

Sistema integrado para gestão de oficina mecânica, desenvolvido como MVP de back-end para o Tech Challenge da pós-graduação.

## Objetivo

O projeto atende os principais requisitos da fase 1:

- CRUD de clientes
- CRUD de veículos
- CRUD de serviços
- CRUD de peças e insumos com controle de estoque
- Criação e acompanhamento de ordens de serviço
- Geração automática de orçamento
- Aprovação, recusa, cancelamento, finalização e entrega da OS
- Consulta pública de OS do cliente por CPF/CNPJ
- Monitoramento do tempo médio de execução
- Autenticação JWT para APIs administrativas
- Documentação da API via Swagger
- Testes unitários e de integração

## Stack utilizada

- ASP.NET Core 10
- Entity Framework Core
- MySQL 8.4
- Docker e Docker Compose
- xUnit
- Swagger 

## Justificativa do banco de dados

O banco de dados escolhido foi o MySQL.

Essa escolha foi feita pelos seguintes motivos:

- familiaridade prévia com a tecnologia, reduzindo a curva de aprendizado e acelerando a implementação do MVP
- suporte adequado ao modelo relacional necessário para clientes, veículos, serviços, peças e ordens de serviço
- integração simples com o Entity Framework Core por meio do provider utilizado no projeto
- facilidade de execução em ambiente Docker, ajudando a padronizar a configuração e a reprodução do sistema em outras máquinas


## Estrutura do projeto

- `OficinaMecanica.Api/`: aplicação principal
- `OficinaMecanica.Api/OficinaMecanica.Api.UnitTests/`: testes unitários
- `OficinaMecanica.Api/OficinaMecanica.Api.IntegrationTests/`: testes de integração
- `docker-compose.yml`: ambiente completo com API e banco

## Como executar

Na raiz do projeto, execute:

```bash
docker compose up --build
```

O ambiente sobe:

- API em `http://localhost:8080`
- Swagger em `http://localhost:8080/swagger`
- MySQL em `localhost:3306`

As migrations são aplicadas automaticamente na inicialização da API.

Para encerrar o ambiente:

```bash
docker compose down
```


## Swagger

O Swagger foi mantido como documentação e teste manual dos endpoints.

URL:

```text
http://localhost:8080/swagger
```

## Autenticação JWT

As APIs administrativas exigem autenticação JWT.

Endpoint de login:

```http
POST /api/auth/login
```

Credenciais padrão:

```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

Após o login, utilize o token retornado no header:

```http
Authorization: Bearer {token}
```

Observação:

- o Swagger está configurado principalmente como documentação;
- para testar rotas autenticadas, o caminho mais simples é usar Postman;
- o Postman pode importar a especificação OpenAPI por:

```text
http://localhost:8080/swagger/v1/swagger.json
```

## Rotas principais

### Autenticação

- `POST /api/auth/login`

### Clientes

- `GET /api/clientes`
- `GET /api/clientes/{id}`
- `POST /api/clientes`
- `PUT /api/clientes/{id}`
- `DELETE /api/clientes/{id}`

### Veículos

- `GET /api/veiculos`
- `GET /api/veiculos/{id}`
- `POST /api/veiculos`
- `PUT /api/veiculos/{id}`
- `DELETE /api/veiculos/{id}`

### Serviços

- `GET /api/servicos`
- `GET /api/servicos/{id}`
- `POST /api/servicos`
- `PUT /api/servicos/{id}`
- `DELETE /api/servicos/{id}`

### Peças e insumos

- `GET /api/pecas`
- `GET /api/pecas/{id}`
- `POST /api/pecas`
- `PUT /api/pecas/{id}`
- `DELETE /api/pecas/{id}`

### Ordens de serviço

- `GET /api/ordensservico`
- `GET /api/ordensservico/{id}`
- `GET /api/ordensservico/tempo-medio-execucao`
- `GET /api/ordensservico/cliente/{cpfCnpj}`
- `POST /api/ordensservico`
- `POST /api/ordensservico/{id}/iniciar-diagnostico`
- `POST /api/ordensservico/{id}/enviar-orcamento`
- `POST /api/ordensservico/{id}/aprovar-orcamento`
- `POST /api/ordensservico/{id}/recusar-orcamento`
- `POST /api/ordensservico/{id}/cancelar`
- `POST /api/ordensservico/{id}/finalizar`
- `POST /api/ordensservico/{id}/entregar`

## Fluxo principal da OS

### 1. Abertura da OS

O atendente abre a OS informando:

- `cpfCnpj`
- `veiculoId`

Exemplo:

```json
{
  "cpfCnpj": "45513451808",
  "veiculoId": "6963ed70-0133-414b-9a8e-7a85f3a67a9c"
}
```

Status inicial:

- `Recebida`

### 2. Início do diagnóstico

Endpoint:

```http
POST /api/ordensservico/{id}/iniciar-diagnostico
```

Status:

- `EmDiagnostico`

### 3. Envio do orçamento

O mecânico informa serviços e peças/insumos após o diagnóstico.

Exemplo:

```json
{
  "servicoIds": [
    "ac668df9-b051-4fbb-8207-8098a7a0ad25"
  ],
  "pecasInsumos": [
    {
      "pecaInsumoId": "91edc7cb-0e36-44ed-8f88-2b8e3037575c",
      "quantidade": 1
    }
  ]
}
```

Endpoint:

```http
POST /api/ordensservico/{id}/enviar-orcamento
```

Comportamento:

- inclui serviços e peças na OS
- calcula o orçamento automaticamente
- registra envio simulado do orçamento ao cliente
- altera o status para `AguardandoAprovacao`

### 4. Aprovação ou recusa

Aprovar:

```http
POST /api/ordensservico/{id}/aprovar-orcamento
```

Status:

- `EmExecucao`

Recusar:

```http
POST /api/ordensservico/{id}/recusar-orcamento
```

Status:

- `EmDiagnostico`

### 5. Cancelamento

```http
POST /api/ordensservico/{id}/cancelar
```

Status:

- `Cancelada`

### 6. Finalização

```http
POST /api/ordensservico/{id}/finalizar
```

Status:

- `Finalizada`

### 7. Entrega

```http
POST /api/ordensservico/{id}/entregar
```

Status:

- `Entregue`

## Regras importantes do sistema

- cliente identificado por `CPF/CNPJ`
- veículo associado a cliente
- `CPF/CNPJ` de cliente não pode duplicar
- placa de veículo validada com biblioteca específica
- `CPF/CNPJ` validado com algoritmo real
- orçamento calculado automaticamente com base nos serviços e peças
- envio de orçamento é simulado, sem integração real com e-mail
- apenas rotas administrativas exigem JWT
- consulta do cliente pode ser feita por:
  - `GET /api/ordensservico/cliente/{cpfCnpj}`




### Todos os testes
Os testes podem ser executados na raiz do repositório.
```bash
dotnet test 
```

### Apenas testes unitários

```bash
dotnet test OficinaMecanica.Api.UnitTests/OficinaMecanica.Api.UnitTests.csproj
```

### Apenas testes de integração

```bash
dotnet test OficinaMecanica.Api.IntegrationTests/OficinaMecanica.Api.IntegrationTests.csproj 
```


## Banco de dados

O MySQL pode ser acessado em:

- host: `localhost`
- porta: `3306`
- database: `oficina_mecanica`
- user: `oficina_user`
- password: `oficina_pass`

Pelo terminal pode rodar: docker compose exec mysql mysql -uoficina_user -poficina_pass oficina_mecanica

## SonarQube

Para gerar o relatório de qualidade do código com SonarQube:

1. Suba o SonarQube na raiz do repositório:

```bash
docker compose -f ../docker-compose.sonar.yml up -d
```

2. Acesse:

```text
http://localhost:9000
```

3. Faça login no SonarQube com:

- usuário: `admin`
- senha: `Admin@123456`

4. Gere um token de acesso no SonarQube.

5. Na pasta da API, exporte o token:

```bash
export SONAR_TOKEN="seu_token_aqui"
```

6. Execute a análise:

```bash
bash scripts/sonar_scan.sh
```

## OWASP ZAP

Para gerar o relatório de segurança da aplicação em execução com OWASP ZAP:

1. Garanta que a aplicação esteja rodando em:

```text
http://localhost:8080
```

2. Na pasta da API, execute:

```bash
bash scripts/zap_scan.sh
```

O script realiza:

- login na API para obter o JWT administrativo
- import da especificação OpenAPI
- scan automatizado da API com ZAP
- geração dos relatórios em:

```text
zap-report/zap-report.html
zap-report/zap-report.json
zap-report/zap-report.xml
```

Observações:

- o script usa as credenciais administrativas padrão da aplicação:
  - usuário: `admin`
  - senha: `Admin@123`
- se essas credenciais mudarem, podem ser sobrescritas antes da execução:

```bash
export ADMIN_USERNAME="admin"
export ADMIN_PASSWORD="nova_senha"
bash scripts/zap_scan.sh
```

- por padrão, o script usa:
  - `http://localhost:8080` para o login feito pela máquina host
  - `http://host.docker.internal:8080` para o acesso do container do ZAP à API


## Observações

- o envio de orçamento é simulado
- para testes autenticados, o Postman tende a oferecer a melhor experiência

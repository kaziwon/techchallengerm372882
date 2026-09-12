# Diagramas de sequencia

## Autenticacao de cliente por CPF

```mermaid
sequenceDiagram
    autonumber
    actor Cliente
    participant URL as Lambda Function URL
    participant Funcao as Lambda Function
    participant UC as AutenticarClienteUseCase
    participant CPF as CpfValidatorGateway
    participant DB as Amazon RDS MySQL
    participant JWT as HmacJwtTokenGateway
    participant Kong as Kong Gateway
    participant API as API Oficina Mecanica

    Cliente->>URL: POST / com CPF
    URL->>Funcao: Evento HTTP
    Funcao->>UC: ExecutarAsync(CPF)
    UC->>CPF: Validar e normalizar

    alt CPF invalido
        CPF-->>UC: Invalido
        UC-->>Funcao: Resultado invalido
        Funcao-->>URL: HTTP 400
        URL-->>Cliente: CPF invalido
    else CPF valido
        UC->>DB: SELECT cliente por CPF
        alt Cliente inexistente
            DB-->>UC: Nenhum registro
            UC-->>Funcao: Cliente inexistente
            Funcao-->>URL: HTTP 404
            URL-->>Cliente: Cliente nao encontrado
        else Cliente inativo
            DB-->>UC: Cliente com Ativo = false
            UC-->>Funcao: Cliente inativo
            Funcao-->>URL: HTTP 403
            URL-->>Cliente: Acesso negado
        else Cliente ativo
            DB-->>UC: Cliente ativo
            UC->>JWT: Emitir token assinado
            JWT-->>UC: JWT e expiracao
            UC-->>Funcao: Cliente, JWT e expiracao
            Funcao-->>URL: HTTP 200
            URL-->>Cliente: JWT e expiracao
        end
    end

    Cliente->>Kong: Requisicao protegida com Bearer JWT
    Kong->>Kong: Validar assinatura, issuer e expiracao
    Kong->>API: Encaminhar requisicao autorizada
    API->>API: Validar novamente o JWT
    API-->>Cliente: Resposta da operacao
```

O JWT e assinado pela Lambda com o mesmo segredo, issuer e audience usados pelo
Kong e pela API. Isso evita uma chamada remota da API para a Lambda em cada
requisicao e preserva duas barreiras de validacao.

## Abertura de ordem de servico

```mermaid
sequenceDiagram
    autonumber
    actor Operador
    participant Kong as Kong Gateway
    participant HTTP as OrdensServicoController HTTP
    participant Clean as OrdensServicoCleanController
    participant UC as CriarOrdemServicoUseCase
    participant Validadores as Gateways de validacao
    participant Repos as Gateways de persistencia
    participant DB as Amazon RDS MySQL
    participant NR as New Relic

    Operador->>Kong: POST /api/ordensservico + Bearer JWT
    Kong->>Kong: Validar JWT e rota
    Kong->>HTTP: Encaminhar JSON
    HTTP->>HTTP: Validar contrato e mapear DTO HTTP
    HTTP->>Clean: Criar(OrdemServicoRequestDto)
    Clean->>UC: Executar(CriarOrdemServicoInput)

    alt Cliente e veiculo existentes
        UC->>Validadores: Validar e normalizar CPF/CNPJ
        UC->>Repos: Buscar cliente e veiculo
        Repos->>DB: SELECT Clientes e Veiculos
        DB-->>Repos: Cadastros encontrados
        UC->>UC: Confirmar que o veiculo pertence ao cliente
    else Cadastro completo na mesma requisicao
        UC->>Validadores: Validar CPF/CNPJ e placa
        UC->>Repos: Verificar duplicidade
        Repos->>DB: SELECT por documento e placa
        UC->>Repos: Adicionar cliente e veiculo
        Repos->>DB: INSERT Clientes e Veiculos
    end

    UC->>Repos: Obter servicos e pecas informados
    Repos->>DB: SELECT catalogos
    UC->>UC: Calcular subtotais e valor do orcamento
    UC->>Repos: Adicionar OS com status Recebida
    Repos->>DB: INSERT OS e itens
    DB-->>Repos: OS persistida
    Repos-->>UC: Entidade criada
    UC-->>Clean: OrdemServicoOutput
    Clean-->>HTTP: OrdemServicoResponseDto
    HTTP->>NR: Log OrdemServicoCriada com correlationId
    HTTP-->>Operador: HTTP 201 + OS
```

O controller HTTP conhece ASP.NET e formatos de transporte. O controller Clean
adapta DTOs para inputs e outputs da aplicacao. O caso de uso concentra as
regras de abertura e depende apenas das interfaces de gateway.

using System.Net;
using System.Net.Http.Json;
using OficinaMecanica.Api.InterfaceAdapters.DTOs;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.IntegrationTests;

public class OrdemServicoEndpointsTests
{
    [Fact]
    public async Task PostOrdemServico_DeveCriarComoRecebida()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client);
        var veiculo = await CriarVeiculo(client, cliente.Id);

        var response = await client.PostAsJsonAsync("/api/ordensservico", new OrdemServicoRequestDto
        {
            CpfCnpj = cliente.CpfCnpj,
            VeiculoId = veiculo.Id
        });
        var responseContent = await response.Content.ReadAsStringAsync();
        var ordem = await response.Content.ReadFromJsonAsync<OrdemServicoResponseDto>(JsonTestOptions.Value);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(ordem);
        Assert.Equal(StatusOrdemServico.Recebida, ordem.Status);
        Assert.Contains("\"status\":\"Recebida\"", responseContent);
        Assert.Empty(ordem.ItensServico);
        Assert.Empty(ordem.ItensPecaInsumo);
        Assert.Equal(0m, ordem.ValorTotalServicos);
        Assert.Equal(0m, ordem.ValorTotalPecasInsumos);
        Assert.Equal(0m, ordem.ValorTotalOrcamento);
        Assert.Equal(string.Empty, ordem.EnvioOrcamento);
    }

    [Fact]
    public async Task PostOrdemServico_DeveCriarClienteEVeiculoQuandoEnviarCadastroCompleto()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var response = await client.PostAsJsonAsync("/api/ordensservico", new OrdemServicoRequestDto
        {
            Cliente = new OrdemServicoClienteRequestDto
            {
                Nome = "Cliente completo",
                CpfCnpj = "39053344705",
                Email = $"{Guid.NewGuid():N}@email.com",
                Telefone = "11999999999"
            },
            Veiculo = new OrdemServicoVeiculoRequestDto
            {
                Placa = "BRA-2E19",
                Marca = "Honda",
                Modelo = "Civic",
                Ano = 2023
            }
        });
        var ordem = await response.Content.ReadFromJsonAsync<OrdemServicoResponseDto>(JsonTestOptions.Value);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(ordem);
        Assert.Equal(StatusOrdemServico.Recebida, ordem.Status);
        Assert.Equal("39053344705", ordem.ClienteCpfCnpj);
        Assert.Equal("BRA2E19", ordem.PlacaVeiculo);
    }

    [Fact]
    public async Task PostOrdemServico_DeveIncluirServicosEPecasQuandoEnviarItensIniciais()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client);
        var veiculo = await CriarVeiculo(client, cliente.Id);

        var response = await client.PostAsJsonAsync("/api/ordensservico", new OrdemServicoRequestDto
        {
            CpfCnpj = cliente.CpfCnpj,
            VeiculoId = veiculo.Id,
            ServicoIds = [servico.Id],
            PecasInsumos =
            [
                new OrdemServicoItemPecaInsumoRequestDto
                {
                    PecaInsumoId = peca.Id,
                    Quantidade = 2
                }
            ]
        });
        var ordem = await response.Content.ReadFromJsonAsync<OrdemServicoResponseDto>(JsonTestOptions.Value);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(ordem);
        Assert.Single(ordem.ItensServico);
        Assert.Single(ordem.ItensPecaInsumo);
        Assert.Equal(120m, ordem.ValorTotalServicos);
        Assert.Equal(60m, ordem.ValorTotalPecasInsumos);
        Assert.Equal(180m, ordem.ValorTotalOrcamento);
        Assert.Equal(StatusOrdemServico.Recebida, ordem.Status);
    }

    [Fact]
    public async Task IniciarDiagnostico_DeveAlterarStatusParaEmDiagnostico()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client);
        var veiculo = await CriarVeiculo(client, cliente.Id);
        var ordem = await CriarOrdemServico(client, cliente, veiculo);

        var response = await client.PostAsync($"/api/ordensservico/{ordem.Id}/iniciar-diagnostico", null);
        var ordemAtualizada = await response.Content.ReadFromJsonAsync<OrdemServicoResponseDto>(JsonTestOptions.Value);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(ordemAtualizada);
        Assert.Equal(StatusOrdemServico.EmDiagnostico, ordemAtualizada.Status);
    }

    [Fact]
    public async Task EnviarOrcamento_DeveAlterarStatusParaAguardandoAprovacao()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client);
        var veiculo = await CriarVeiculo(client, cliente.Id);
        var ordem = await CriarOrdemServico(client, cliente, veiculo);

        await client.PostAsync($"/api/ordensservico/{ordem.Id}/iniciar-diagnostico", null);

        var response = await EnviarOrcamento(client, ordem.Id, servico.Id, peca.Id);
        var ordemAtualizada = await response.Content.ReadFromJsonAsync<OrdemServicoResponseDto>(JsonTestOptions.Value);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(ordemAtualizada);
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, ordemAtualizada.Status);
        Assert.Single(ordemAtualizada.ItensServico);
        Assert.Single(ordemAtualizada.ItensPecaInsumo);
        Assert.Contains("Orcamento enviado", ordemAtualizada.EnvioOrcamento);
    }

    [Fact]
    public async Task EnviarOrcamento_DeveRetornarConflict_QuandoOrdemNaoEstiverEmDiagnostico()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client);
        var veiculo = await CriarVeiculo(client, cliente.Id);
        var ordem = await CriarOrdemServico(client, cliente, veiculo);

        var response = await EnviarOrcamento(client, ordem.Id, servico.Id, peca.Id);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AprovarOrcamento_DeveAlterarStatusParaEmExecucao()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client);
        var veiculo = await CriarVeiculo(client, cliente.Id);
        var ordem = await CriarOrdemServico(client, cliente, veiculo);

        await client.PostAsync($"/api/ordensservico/{ordem.Id}/iniciar-diagnostico", null);
        await EnviarOrcamento(client, ordem.Id, servico.Id, peca.Id);

        var response = await client.PostAsync($"/api/ordensservico/{ordem.Id}/aprovar-orcamento", null);
        var ordemAtualizada = await response.Content.ReadFromJsonAsync<OrdemServicoResponseDto>(JsonTestOptions.Value);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(ordemAtualizada);
        Assert.Equal(StatusOrdemServico.EmExecucao, ordemAtualizada.Status);
        Assert.Equal(StatusAprovacaoOrcamento.Aprovado, ordemAtualizada.StatusAprovacaoOrcamento);
    }

    [Fact]
    public async Task AprovarOrcamento_DeveRetornarConflict_QuandoEstoqueForInsuficiente()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client, 0);
        var veiculo = await CriarVeiculo(client, cliente.Id);
        var ordem = await CriarOrdemServico(client, cliente, veiculo);

        await client.PostAsync($"/api/ordensservico/{ordem.Id}/iniciar-diagnostico", null);
        await EnviarOrcamento(client, ordem.Id, servico.Id, peca.Id);

        var response = await client.PostAsync($"/api/ordensservico/{ordem.Id}/aprovar-orcamento", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task RecusarOrcamento_DeveVoltarParaDiagnostico()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client);
        var veiculo = await CriarVeiculo(client, cliente.Id);
        var ordem = await CriarOrdemServico(client, cliente, veiculo);

        await client.PostAsync($"/api/ordensservico/{ordem.Id}/iniciar-diagnostico", null);
        await EnviarOrcamento(client, ordem.Id, servico.Id, peca.Id);

        var response = await client.PostAsJsonAsync($"/api/ordensservico/{ordem.Id}/recusar-orcamento", new OrdemServicoRespostaAprovacaoRequestDto
        {
            MotivoRecusa = "Quero revisar o valor"
        });
        var ordemAtualizada = await response.Content.ReadFromJsonAsync<OrdemServicoResponseDto>(JsonTestOptions.Value);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(ordemAtualizada);
        Assert.Equal(StatusOrdemServico.EmDiagnostico, ordemAtualizada.Status);
        Assert.Equal(StatusAprovacaoOrcamento.Recusado, ordemAtualizada.StatusAprovacaoOrcamento);
    }

    [Fact]
    public async Task Cancelar_DeveAlterarStatusParaCancelada()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client);
        var veiculo = await CriarVeiculo(client, cliente.Id);
        var ordem = await CriarOrdemServico(client, cliente, veiculo);

        await client.PostAsync($"/api/ordensservico/{ordem.Id}/iniciar-diagnostico", null);
        await EnviarOrcamento(client, ordem.Id, servico.Id, peca.Id);
        await client.PostAsync($"/api/ordensservico/{ordem.Id}/aprovar-orcamento", null);

        var response = await client.PostAsync($"/api/ordensservico/{ordem.Id}/cancelar", null);
        var ordemAtualizada = await response.Content.ReadFromJsonAsync<OrdemServicoResponseDto>(JsonTestOptions.Value);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(ordemAtualizada);
        Assert.Equal(StatusOrdemServico.Cancelada, ordemAtualizada.Status);
    }

    [Fact]
    public async Task Cancelar_DeveRetornarConflict_QuandoOrdemJaEstiverFinalizada()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client);
        var veiculo = await CriarVeiculo(client, cliente.Id);
        var ordem = await CriarOrdemServico(client, cliente, veiculo);

        await client.PostAsync($"/api/ordensservico/{ordem.Id}/iniciar-diagnostico", null);
        await EnviarOrcamento(client, ordem.Id, servico.Id, peca.Id);
        await client.PostAsync($"/api/ordensservico/{ordem.Id}/aprovar-orcamento", null);
        await client.PostAsync($"/api/ordensservico/{ordem.Id}/finalizar", null);

        var response = await client.PostAsync($"/api/ordensservico/{ordem.Id}/cancelar", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Entregar_DeveAlterarStatusParaEntregue()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client);
        var veiculo = await CriarVeiculo(client, cliente.Id);
        var ordem = await CriarOrdemServico(client, cliente, veiculo);

        await client.PostAsync($"/api/ordensservico/{ordem.Id}/iniciar-diagnostico", null);
        await EnviarOrcamento(client, ordem.Id, servico.Id, peca.Id);
        await client.PostAsync($"/api/ordensservico/{ordem.Id}/aprovar-orcamento", null);
        await client.PostAsync($"/api/ordensservico/{ordem.Id}/finalizar", null);

        var response = await client.PostAsync($"/api/ordensservico/{ordem.Id}/entregar", null);
        var ordemAtualizada = await response.Content.ReadFromJsonAsync<OrdemServicoResponseDto>(JsonTestOptions.Value);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(ordemAtualizada);
        Assert.Equal(StatusOrdemServico.Entregue, ordemAtualizada.Status);
    }

    [Fact]
    public async Task GetPorCpfCnpjCliente_DeveRetornarOrdensDoCliente()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var adminClient = factory.CreateClient();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(adminClient);

        var cliente = await CriarCliente(adminClient);
        var servico = await CriarServico(adminClient);
        var peca = await CriarPeca(adminClient);
        var veiculo = await CriarVeiculo(adminClient, cliente.Id);
        await CriarOrdemServico(adminClient, cliente, veiculo);

        var response = await client.GetAsync($"/api/ordensservico/cliente/{cliente.CpfCnpj}");
        var ordens = await response.Content.ReadFromJsonAsync<List<OrdemServicoResponseDto>>(JsonTestOptions.Value);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(ordens);
        Assert.NotEmpty(ordens);
        Assert.All(ordens, ordem => Assert.Equal(cliente.CpfCnpj, ordem.ClienteCpfCnpj));
    }

    [Fact]
    public async Task GetTempoMedioExecucao_DeveRetornarMetricaDasOrdensFinalizadas()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client);
        var veiculo = await CriarVeiculo(client, cliente.Id);
        var ordem = await CriarOrdemServico(client, cliente, veiculo);

        await client.PostAsync($"/api/ordensservico/{ordem.Id}/iniciar-diagnostico", null);
        await EnviarOrcamento(client, ordem.Id, servico.Id, peca.Id);
        await client.PostAsync($"/api/ordensservico/{ordem.Id}/aprovar-orcamento", null);
        await client.PostAsync($"/api/ordensservico/{ordem.Id}/finalizar", null);

        var response = await client.GetAsync("/api/ordensservico/tempo-medio-execucao");
        var metrica = await response.Content.ReadFromJsonAsync<TempoMedioExecucaoResponseDto>(JsonTestOptions.Value);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(metrica);
        Assert.Equal(1, metrica.QuantidadeOrdensConsideradas);
        Assert.True(metrica.TempoMedioExecucaoEmMinutos >= 0);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoOrdemNaoExistir()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var response = await client.GetAsync($"/api/ordensservico/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<ClienteResponseDto> CriarCliente(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/clientes", new ClienteRequestDto
        {
            Nome = "Cliente OS",
            CpfCnpj = "52998224725",
            Email = $"{Guid.NewGuid():N}@email.com",
            Telefone = "11999999999"
        });

        return (await response.Content.ReadFromJsonAsync<ClienteResponseDto>(JsonTestOptions.Value))!;
    }

    private static async Task<ServicoResponseDto> CriarServico(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/servicos", new ServicoRequestDto
        {
            Nome = $"Servico-{Guid.NewGuid():N}",
            Descricao = "Servico de teste",
            Preco = 120m
        });

        return (await response.Content.ReadFromJsonAsync<ServicoResponseDto>(JsonTestOptions.Value))!;
    }

    private static async Task<PecaInsumoResponseDto> CriarPeca(HttpClient client, int quantidadeEstoque = 10)
    {
        var response = await client.PostAsJsonAsync("/api/pecas", new PecaInsumoRequestDto
        {
            Nome = $"Peca-{Guid.NewGuid():N}",
            Descricao = "Peca de teste",
            PrecoUnitario = 30m,
            QuantidadeEstoque = quantidadeEstoque
        });

        return (await response.Content.ReadFromJsonAsync<PecaInsumoResponseDto>(JsonTestOptions.Value))!;
    }

    private static async Task<VeiculoResponseDto> CriarVeiculo(HttpClient client, Guid clienteId)
    {
        var response = await client.PostAsJsonAsync("/api/veiculos", new VeiculoRequestDto
        {
            ClienteId = clienteId,
            Placa = $"BRA{Random.Shared.Next(1, 9)}E{Random.Shared.Next(10, 99)}",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2022
        });

        return (await response.Content.ReadFromJsonAsync<VeiculoResponseDto>(JsonTestOptions.Value))!;
    }

    private static async Task<OrdemServicoResponseDto> CriarOrdemServico(
        HttpClient client,
        ClienteResponseDto cliente,
        VeiculoResponseDto veiculo)
    {
        var response = await client.PostAsJsonAsync("/api/ordensservico", new OrdemServicoRequestDto
        {
            CpfCnpj = cliente.CpfCnpj,
            VeiculoId = veiculo.Id
        });

        return (await response.Content.ReadFromJsonAsync<OrdemServicoResponseDto>(JsonTestOptions.Value))!;
    }

    private static Task<HttpResponseMessage> EnviarOrcamento(HttpClient client, Guid ordemServicoId, Guid servicoId, Guid pecaInsumoId)
    {
        return client.PostAsJsonAsync($"/api/ordensservico/{ordemServicoId}/enviar-orcamento", new OrdemServicoOrcamentoRequestDto
        {
            ServicoIds = [servicoId],
            PecasInsumos =
            [
                new OrdemServicoItemPecaInsumoRequestDto
                {
                    PecaInsumoId = pecaInsumoId,
                    Quantidade = 1
                }
            ]
        });
    }
}

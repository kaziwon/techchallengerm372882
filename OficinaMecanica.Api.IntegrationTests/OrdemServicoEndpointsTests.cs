using System.Net;
using System.Net.Http.Json;
using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.IntegrationTests;

public class OrdemServicoEndpointsTests
{
    [Fact]
    public async Task PostOrdemServico_DeveCriarComoRecebida()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

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
        var responseContent = await response.Content.ReadAsStringAsync();
        var ordem = await response.Content.ReadFromJsonAsync<OrdemServicoResponseDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(ordem);
        Assert.Equal(StatusOrdemServico.Recebida, ordem.Status);
        Assert.Contains("\"status\":\"Recebida\"", responseContent);
        Assert.Equal(string.Empty, ordem.EnvioOrcamento);
    }

    [Fact]
    public async Task IniciarDiagnostico_DeveAlterarStatusParaEmDiagnostico()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client);
        var veiculo = await CriarVeiculo(client, cliente.Id);
        var ordem = await CriarOrdemServico(client, cliente, veiculo, servico, peca);

        var response = await client.PostAsync($"/api/ordensservico/{ordem.Id}/iniciar-diagnostico", null);
        var ordemAtualizada = await response.Content.ReadFromJsonAsync<OrdemServicoResponseDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(ordemAtualizada);
        Assert.Equal(StatusOrdemServico.EmDiagnostico, ordemAtualizada.Status);
    }

    [Fact]
    public async Task EnviarOrcamento_DeveAlterarStatusParaAguardandoAprovacao()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client);
        var veiculo = await CriarVeiculo(client, cliente.Id);
        var ordem = await CriarOrdemServico(client, cliente, veiculo, servico, peca);

        await client.PostAsync($"/api/ordensservico/{ordem.Id}/iniciar-diagnostico", null);

        var response = await client.PostAsync($"/api/ordensservico/{ordem.Id}/enviar-orcamento", null);
        var ordemAtualizada = await response.Content.ReadFromJsonAsync<OrdemServicoResponseDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(ordemAtualizada);
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, ordemAtualizada.Status);
        Assert.Contains("Orcamento enviado", ordemAtualizada.EnvioOrcamento);
    }

    [Fact]
    public async Task AprovarOrcamento_DeveAlterarStatusParaEmExecucao()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client);
        var veiculo = await CriarVeiculo(client, cliente.Id);
        var ordem = await CriarOrdemServico(client, cliente, veiculo, servico, peca);

        await client.PostAsync($"/api/ordensservico/{ordem.Id}/iniciar-diagnostico", null);
        await client.PostAsync($"/api/ordensservico/{ordem.Id}/enviar-orcamento", null);

        var response = await client.PostAsync($"/api/ordensservico/{ordem.Id}/aprovar-orcamento", null);
        var ordemAtualizada = await response.Content.ReadFromJsonAsync<OrdemServicoResponseDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(ordemAtualizada);
        Assert.Equal(StatusOrdemServico.EmExecucao, ordemAtualizada.Status);
        Assert.Equal(StatusAprovacaoOrcamento.Aprovado, ordemAtualizada.StatusAprovacaoOrcamento);
    }

    [Fact]
    public async Task RecusarOrcamento_DeveVoltarParaDiagnostico()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client);
        var veiculo = await CriarVeiculo(client, cliente.Id);
        var ordem = await CriarOrdemServico(client, cliente, veiculo, servico, peca);

        await client.PostAsync($"/api/ordensservico/{ordem.Id}/iniciar-diagnostico", null);
        await client.PostAsync($"/api/ordensservico/{ordem.Id}/enviar-orcamento", null);

        var response = await client.PostAsJsonAsync($"/api/ordensservico/{ordem.Id}/recusar-orcamento", new OrdemServicoRespostaAprovacaoRequestDto
        {
            MotivoRecusa = "Quero revisar o valor"
        });
        var ordemAtualizada = await response.Content.ReadFromJsonAsync<OrdemServicoResponseDto>();

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

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client);
        var veiculo = await CriarVeiculo(client, cliente.Id);
        var ordem = await CriarOrdemServico(client, cliente, veiculo, servico, peca);

        await client.PostAsync($"/api/ordensservico/{ordem.Id}/iniciar-diagnostico", null);
        await client.PostAsync($"/api/ordensservico/{ordem.Id}/enviar-orcamento", null);
        await client.PostAsync($"/api/ordensservico/{ordem.Id}/aprovar-orcamento", null);

        var response = await client.PostAsync($"/api/ordensservico/{ordem.Id}/cancelar", null);
        var ordemAtualizada = await response.Content.ReadFromJsonAsync<OrdemServicoResponseDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(ordemAtualizada);
        Assert.Equal(StatusOrdemServico.Cancelada, ordemAtualizada.Status);
    }

    [Fact]
    public async Task GetPorCpfCnpjCliente_DeveRetornarOrdensDoCliente()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var cliente = await CriarCliente(client);
        var servico = await CriarServico(client);
        var peca = await CriarPeca(client);
        var veiculo = await CriarVeiculo(client, cliente.Id);
        await CriarOrdemServico(client, cliente, veiculo, servico, peca);

        var response = await client.GetAsync($"/api/ordensservico/cliente/{cliente.CpfCnpj}");
        var ordens = await response.Content.ReadFromJsonAsync<List<OrdemServicoResponseDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(ordens);
        Assert.NotEmpty(ordens);
        Assert.All(ordens, ordem => Assert.Equal(cliente.CpfCnpj, ordem.ClienteCpfCnpj));
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

        return (await response.Content.ReadFromJsonAsync<ClienteResponseDto>())!;
    }

    private static async Task<ServicoResponseDto> CriarServico(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/servicos", new ServicoRequestDto
        {
            Nome = $"Servico-{Guid.NewGuid():N}",
            Descricao = "Servico de teste",
            Preco = 120m
        });

        return (await response.Content.ReadFromJsonAsync<ServicoResponseDto>())!;
    }

    private static async Task<PecaInsumoResponseDto> CriarPeca(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/pecas", new PecaInsumoRequestDto
        {
            Nome = $"Peca-{Guid.NewGuid():N}",
            Descricao = "Peca de teste",
            PrecoUnitario = 30m,
            QuantidadeEstoque = 10
        });

        return (await response.Content.ReadFromJsonAsync<PecaInsumoResponseDto>())!;
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

        return (await response.Content.ReadFromJsonAsync<VeiculoResponseDto>())!;
    }

    private static async Task<OrdemServicoResponseDto> CriarOrdemServico(
        HttpClient client,
        ClienteResponseDto cliente,
        VeiculoResponseDto veiculo,
        ServicoResponseDto servico,
        PecaInsumoResponseDto peca)
    {
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
                    Quantidade = 1
                }
            ]
        });

        return (await response.Content.ReadFromJsonAsync<OrdemServicoResponseDto>())!;
    }
}

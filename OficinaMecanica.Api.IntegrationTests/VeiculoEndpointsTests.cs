using System.Net;
using System.Net.Http.Json;
using OficinaMecanica.Api.InterfaceAdapters.DTOs;

namespace OficinaMecanica.Api.IntegrationTests;

public class VeiculoEndpointsTests
{
    [Fact]
    public async Task PostVeiculo_DeveRetornarBadRequest_QuandoPlacaForInvalida()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var cliente = await CriarCliente(client);

        var response = await client.PostAsJsonAsync("/api/veiculos", new VeiculoRequestDto
        {
            ClienteId = cliente.Id,
            Placa = "123",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2022
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostVeiculo_DeveRetornarConflict_QuandoPlacaForDuplicada()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var cliente = await CriarCliente(client);
        var request = new VeiculoRequestDto
        {
            ClienteId = cliente.Id,
            Placa = "BRA2E19",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2022
        };

        await client.PostAsJsonAsync("/api/veiculos", request);
        var response = await client.PostAsJsonAsync("/api/veiculos", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private static async Task<ClienteResponseDto> CriarCliente(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/clientes", new ClienteRequestDto
        {
            Nome = "Cliente Veiculo",
            CpfCnpj = "39053344705",
            Email = $"{Guid.NewGuid():N}@email.com",
            Telefone = "11999999999"
        });

        return (await response.Content.ReadFromJsonAsync<ClienteResponseDto>(JsonTestOptions.Value))!;
    }
}

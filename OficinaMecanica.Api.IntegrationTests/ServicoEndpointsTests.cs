using System.Net;
using System.Net.Http.Json;
using OficinaMecanica.Api.Application.DTOs;

namespace OficinaMecanica.Api.IntegrationTests;

public class ServicoEndpointsTests
{
    [Fact]
    public async Task PostServico_DeveRetornarConflict_QuandoNomeForDuplicado()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var request = new ServicoRequestDto
        {
            Nome = $"Servico-{Guid.NewGuid():N}",
            Descricao = "Servico de teste",
            Preco = 100m
        };

        await client.PostAsJsonAsync("/api/servicos", request);
        var response = await client.PostAsJsonAsync("/api/servicos", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}

using System.Net;
using System.Net.Http.Json;
using OficinaMecanica.Api.Application.DTOs;

namespace OficinaMecanica.Api.IntegrationTests;

public class PecaEndpointsTests
{
    [Fact]
    public async Task PostPeca_DeveRetornarConflict_QuandoNomeForDuplicado()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var request = new PecaInsumoRequestDto
        {
            Nome = $"Peca-{Guid.NewGuid():N}",
            Descricao = "Peca de teste",
            PrecoUnitario = 25m,
            QuantidadeEstoque = 5
        };

        await client.PostAsJsonAsync("/api/pecas", request);
        var response = await client.PostAsJsonAsync("/api/pecas", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}

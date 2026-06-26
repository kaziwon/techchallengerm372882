using System.Net;
using System.Net.Http.Json;
using OficinaMecanica.Api.InterfaceAdapters.DTOs;

namespace OficinaMecanica.Api.IntegrationTests;

public class AuthEndpointsTests
{
    [Fact]
    public async Task Login_DeveRetornarToken_QuandoCredenciaisForemValidas()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Username = "admin",
            Password = "Admin@123"
        });
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>(JsonTestOptions.Value);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(loginResponse);
        Assert.False(string.IsNullOrWhiteSpace(loginResponse.Token));
    }

    [Fact]
    public async Task GetClientes_DeveRetornarUnauthorized_QuandoNaoEnviarToken()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

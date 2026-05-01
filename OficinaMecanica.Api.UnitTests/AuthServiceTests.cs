using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.Services;
using OficinaMecanica.Api.Application.Settings;

namespace OficinaMecanica.Api.UnitTests;

public class AuthServiceTests
{
    [Fact]
    public void Login_DeveRetornarNull_QuandoCredenciaisForemInvalidas()
    {
        var service = CriarService();

        var response = service.Login(new LoginRequestDto
        {
            Username = "admin",
            Password = "senha-errada"
        });

        Assert.Null(response);
    }

    [Fact]
    public void Login_DeveRetornarTokenValido_QuandoCredenciaisForemValidas()
    {
        var service = CriarService();

        var response = service.Login(new LoginRequestDto
        {
            Username = "admin",
            Password = "Admin@123"
        });

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.True(response.ExpiresAt > DateTime.UtcNow);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);

        Assert.Equal("OficinaMecanica.Api", token.Issuer);
        Assert.Contains(token.Audiences, audience => audience == "OficinaMecanica.Api");
        Assert.Contains(token.Claims, claim => claim.Type == ClaimTypes.Name && claim.Value == "admin");
        Assert.Contains(token.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
    }

    private static AuthService CriarService()
    {
        return new AuthService(
            Options.Create(new JwtSettings
            {
                Issuer = "OficinaMecanica.Api",
                Audience = "OficinaMecanica.Api",
                SecretKey = "oficina-mecanica-tech-challenge-secret-key-2026"
            }),
            Options.Create(new AdminUserSettings
            {
                Username = "admin",
                Password = "Admin@123"
            }));
    }
}

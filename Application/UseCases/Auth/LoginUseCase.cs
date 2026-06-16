using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.Api.Application.Settings;

namespace OficinaMecanica.Api.Application.UseCases.Auth;

public record LoginInput(string Username, string Password);

public class LoginOutput
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class LoginUseCase
{
    private readonly JwtSettings _jwtSettings;
    private readonly AdminUserSettings _adminUserSettings;

    public LoginUseCase(IOptions<JwtSettings> jwtSettings, IOptions<AdminUserSettings> adminUserSettings)
    {
        _jwtSettings = jwtSettings.Value;
        _adminUserSettings = adminUserSettings.Value;
    }

    public LoginOutput? Executar(LoginInput input)
    {
        if (input.Username != _adminUserSettings.Username || input.Password != _adminUserSettings.Password)
        {
            return null;
        }

        var expiresAt = DateTime.UtcNow.AddHours(2);
        Claim[] claims =
        [
            new Claim(ClaimTypes.Name, input.Username),
            new Claim(ClaimTypes.Role, "Admin")
        ];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return new LoginOutput
        {
            Token = token,
            ExpiresAt = expiresAt
        };
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.Interfaces;
using OficinaMecanica.Api.Application.Settings;

namespace OficinaMecanica.Api.Application.Services;

public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly AdminUserSettings _adminUserSettings;

    public AuthService(IOptions<JwtSettings> jwtSettings, IOptions<AdminUserSettings> adminUserSettings)
    {
        _jwtSettings = jwtSettings.Value;
        _adminUserSettings = adminUserSettings.Value;
    }

    public LoginResponseDto? Login(LoginRequestDto loginRequestDto)
    {
        if (loginRequestDto.Username != _adminUserSettings.Username || loginRequestDto.Password != _adminUserSettings.Password)
        {
            return null;
        }

        var expiresAt = DateTime.UtcNow.AddHours(2);
        Claim[] claims =
        [
            new Claim(ClaimTypes.Name, loginRequestDto.Username),
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

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt
        };
    }
}

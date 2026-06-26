namespace OficinaMecanica.Api.InterfaceAdapters.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

namespace OficinaMecanica.Api.Application.UseCases.Auth;

public class LoginOutput
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

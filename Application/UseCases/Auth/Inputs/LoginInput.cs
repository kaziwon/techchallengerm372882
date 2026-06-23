namespace OficinaMecanica.Api.Application.UseCases.Auth;

public class LoginInput
{
    public LoginInput(string username, string password)
    {
        Username = username;
        Password = password;
    }

    public string Username { get; }
    public string Password { get; }
}

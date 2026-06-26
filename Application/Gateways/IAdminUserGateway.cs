namespace OficinaMecanica.Api.Application.Gateways;

public interface IAdminUserGateway
{
    bool CredenciaisValidas(string username, string password);
}

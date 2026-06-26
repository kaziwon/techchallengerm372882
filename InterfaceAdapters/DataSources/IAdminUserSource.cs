namespace OficinaMecanica.Api.InterfaceAdapters.DataSources;

public interface IAdminUserSource
{
    bool CredenciaisValidas(string username, string password);
}

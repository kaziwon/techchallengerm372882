using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.InterfaceAdapters.DataSources;

namespace OficinaMecanica.Api.InterfaceAdapters.Gateways;

public class AdminUserGateway : IAdminUserGateway
{
    private readonly IAdminUserSource _adminUserSource;

    public AdminUserGateway(IAdminUserSource adminUserSource)
    {
        _adminUserSource = adminUserSource;
    }

    public bool CredenciaisValidas(string username, string password)
    {
        return _adminUserSource.CredenciaisValidas(username, password);
    }
}

using Microsoft.Extensions.Options;
using OficinaMecanica.Api.Infrastructure.Settings;
using OficinaMecanica.Api.InterfaceAdapters.DataSources;

namespace OficinaMecanica.Api.Infrastructure.Sources;

public class AdminUserSettingsSource : IAdminUserSource
{
    private readonly AdminUserSettings _adminUserSettings;

    public AdminUserSettingsSource(IOptions<AdminUserSettings> adminUserSettings)
    {
        _adminUserSettings = adminUserSettings.Value;
    }

    public bool CredenciaisValidas(string username, string password)
    {
        return username == _adminUserSettings.Username && password == _adminUserSettings.Password;
    }
}

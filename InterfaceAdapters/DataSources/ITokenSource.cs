using OficinaMecanica.Api.Application.Gateways;

namespace OficinaMecanica.Api.InterfaceAdapters.DataSources;

public interface ITokenSource
{
    TokenGerado GerarToken(string username, string role);
}

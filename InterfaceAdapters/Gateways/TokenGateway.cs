using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.InterfaceAdapters.DataSources;

namespace OficinaMecanica.Api.InterfaceAdapters.Gateways;

public class TokenGateway : ITokenGateway
{
    private readonly ITokenSource _tokenSource;

    public TokenGateway(ITokenSource tokenSource)
    {
        _tokenSource = tokenSource;
    }

    public TokenGerado GerarToken(string username, string role)
    {
        return _tokenSource.GerarToken(username, role);
    }
}

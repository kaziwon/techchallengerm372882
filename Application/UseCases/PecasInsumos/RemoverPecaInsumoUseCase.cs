using OficinaMecanica.Api.Application.Gateways;

namespace OficinaMecanica.Api.Application.UseCases.PecasInsumos;

public class RemoverPecaInsumoUseCase
{
    private readonly IPecaInsumoGateway _pecaInsumoGateway;

    public RemoverPecaInsumoUseCase(IPecaInsumoGateway pecaInsumoGateway)
    {
        _pecaInsumoGateway = pecaInsumoGateway;
    }

    public bool Executar(Guid id)
    {
        return _pecaInsumoGateway.Remover(id);
    }
}

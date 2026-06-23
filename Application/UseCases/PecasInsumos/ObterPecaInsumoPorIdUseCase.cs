using OficinaMecanica.Api.Application.Gateways;

namespace OficinaMecanica.Api.Application.UseCases.PecasInsumos;

public class ObterPecaInsumoPorIdUseCase
{
    private readonly IPecaInsumoGateway _pecaInsumoGateway;

    public ObterPecaInsumoPorIdUseCase(IPecaInsumoGateway pecaInsumoGateway)
    {
        _pecaInsumoGateway = pecaInsumoGateway;
    }

    public PecaInsumoOutput? Executar(Guid id)
    {
        var pecaInsumo = _pecaInsumoGateway.ObterPorId(id);

        return pecaInsumo is null ? null : PecaInsumoOutputMapper.Mapear(pecaInsumo);
    }
}

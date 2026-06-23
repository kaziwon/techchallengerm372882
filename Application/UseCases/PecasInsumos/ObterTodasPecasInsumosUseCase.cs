using OficinaMecanica.Api.Application.Gateways;

namespace OficinaMecanica.Api.Application.UseCases.PecasInsumos;

public class ObterTodasPecasInsumosUseCase
{
    private readonly IPecaInsumoGateway _pecaInsumoGateway;

    public ObterTodasPecasInsumosUseCase(IPecaInsumoGateway pecaInsumoGateway)
    {
        _pecaInsumoGateway = pecaInsumoGateway;
    }

    public List<PecaInsumoOutput> Executar()
    {
        return _pecaInsumoGateway.ObterTodos().Select(PecaInsumoOutputMapper.Mapear).ToList();
    }
}

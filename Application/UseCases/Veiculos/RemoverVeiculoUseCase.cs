using OficinaMecanica.Api.Application.Gateways;

namespace OficinaMecanica.Api.Application.UseCases.Veiculos;

public class RemoverVeiculoUseCase
{
    private readonly IVeiculoGateway _veiculoGateway;

    public RemoverVeiculoUseCase(IVeiculoGateway veiculoGateway)
    {
        _veiculoGateway = veiculoGateway;
    }

    public bool Executar(Guid id)
    {
        return _veiculoGateway.Remover(id);
    }
}

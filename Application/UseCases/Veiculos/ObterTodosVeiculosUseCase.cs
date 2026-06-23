using OficinaMecanica.Api.Application.Gateways;

namespace OficinaMecanica.Api.Application.UseCases.Veiculos;

public class ObterTodosVeiculosUseCase
{
    private readonly IVeiculoGateway _veiculoGateway;

    public ObterTodosVeiculosUseCase(IVeiculoGateway veiculoGateway)
    {
        _veiculoGateway = veiculoGateway;
    }

    public List<VeiculoOutput> Executar()
    {
        return _veiculoGateway.ObterTodos().Select(VeiculoOutputMapper.Mapear).ToList();
    }
}

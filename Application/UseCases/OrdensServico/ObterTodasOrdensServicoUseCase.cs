using OficinaMecanica.Api.Application.Gateways;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class ObterTodasOrdensServicoUseCase
{
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public ObterTodasOrdensServicoUseCase(IOrdemServicoGateway ordemServicoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
    }

    public List<OrdemServicoOutput> Executar()
    {
        return _ordemServicoGateway.ObterTodas().Select(OrdemServicoOutputMapper.Mapear).ToList();
    }
}

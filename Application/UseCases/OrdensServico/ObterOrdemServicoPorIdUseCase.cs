using OficinaMecanica.Api.Application.Gateways;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class ObterOrdemServicoPorIdUseCase
{
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public ObterOrdemServicoPorIdUseCase(IOrdemServicoGateway ordemServicoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
    }

    public OrdemServicoOutput? Executar(Guid id)
    {
        var ordemServico = _ordemServicoGateway.ObterPorId(id);

        return ordemServico is null ? null : OrdemServicoOutputMapper.Mapear(ordemServico);
    }
}

using OficinaMecanica.Api.Application.Gateways;

namespace OficinaMecanica.Api.Application.UseCases.Servicos;

public class ObterServicoPorIdUseCase
{
    private readonly IServicoGateway _servicoGateway;

    public ObterServicoPorIdUseCase(IServicoGateway servicoGateway)
    {
        _servicoGateway = servicoGateway;
    }

    public ServicoOutput? Executar(Guid id)
    {
        var servico = _servicoGateway.ObterPorId(id);

        return servico is null ? null : ServicoOutputMapper.Mapear(servico);
    }
}

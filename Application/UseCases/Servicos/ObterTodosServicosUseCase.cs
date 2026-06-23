using OficinaMecanica.Api.Application.Gateways;

namespace OficinaMecanica.Api.Application.UseCases.Servicos;

public class ObterTodosServicosUseCase
{
    private readonly IServicoGateway _servicoGateway;

    public ObterTodosServicosUseCase(IServicoGateway servicoGateway)
    {
        _servicoGateway = servicoGateway;
    }

    public List<ServicoOutput> Executar()
    {
        return _servicoGateway.ObterTodos().Select(ServicoOutputMapper.Mapear).ToList();
    }
}

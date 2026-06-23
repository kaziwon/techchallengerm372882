using OficinaMecanica.Api.Application.Gateways;

namespace OficinaMecanica.Api.Application.UseCases.Servicos;

public class RemoverServicoUseCase
{
    private readonly IServicoGateway _servicoGateway;

    public RemoverServicoUseCase(IServicoGateway servicoGateway)
    {
        _servicoGateway = servicoGateway;
    }

    public bool Executar(Guid id)
    {
        return _servicoGateway.Remover(id);
    }
}

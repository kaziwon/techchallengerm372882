using OficinaMecanica.Api.Application.Gateways;

namespace OficinaMecanica.Api.Application.UseCases.Clientes;

public class RemoverClienteUseCase
{
    private readonly IClienteGateway _clienteGateway;

    public RemoverClienteUseCase(IClienteGateway clienteGateway)
    {
        _clienteGateway = clienteGateway;
    }

    public bool Executar(Guid id)
    {
        return _clienteGateway.Remover(id);
    }
}

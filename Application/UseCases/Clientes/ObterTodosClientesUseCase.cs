using OficinaMecanica.Api.Application.Gateways;

namespace OficinaMecanica.Api.Application.UseCases.Clientes;

public class ObterTodosClientesUseCase
{
    private readonly IClienteGateway _clienteGateway;

    public ObterTodosClientesUseCase(IClienteGateway clienteGateway)
    {
        _clienteGateway = clienteGateway;
    }

    public List<ClienteOutput> Executar()
    {
        return _clienteGateway
            .ObterTodos()
            .Select(ClienteOutputMapper.Mapear)
            .ToList();
    }
}

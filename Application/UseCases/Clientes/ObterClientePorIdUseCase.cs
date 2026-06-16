using OficinaMecanica.Api.Application.Gateways;

namespace OficinaMecanica.Api.Application.UseCases.Clientes;

public class ObterClientePorIdUseCase
{
    private readonly IClienteGateway _clienteGateway;

    public ObterClientePorIdUseCase(IClienteGateway clienteGateway)
    {
        _clienteGateway = clienteGateway;
    }

    public ClienteOutput? Executar(Guid id)
    {
        var cliente = _clienteGateway.ObterPorId(id);

        return cliente is null ? null : ClienteOutputMapper.Mapear(cliente);
    }
}

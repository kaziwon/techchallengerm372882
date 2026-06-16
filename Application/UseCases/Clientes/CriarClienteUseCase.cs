using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.Clientes;

public class CriarClienteUseCase
{
    private const string CpfCnpjJaCadastrado = "Ja existe um cliente cadastrado com este CPF/CNPJ.";
    private readonly IClienteGateway _clienteGateway;

    public CriarClienteUseCase(IClienteGateway clienteGateway)
    {
        _clienteGateway = clienteGateway;
    }

    public ClienteOutput Executar(CriarClienteInput input)
    {
        if (_clienteGateway.ExistePorCpfCnpj(input.CpfCnpj))
        {
            throw new InvalidOperationException(CpfCnpjJaCadastrado);
        }

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = input.Nome,
            CpfCnpj = input.CpfCnpj,
            Email = input.Email,
            Telefone = input.Telefone
        };

        var clienteAdicionado = _clienteGateway.Adicionar(cliente);

        return ClienteOutputMapper.Mapear(clienteAdicionado);
    }
}

using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.Clientes;

public class AtualizarClienteUseCase
{
    private const string CpfCnpjJaCadastrado = "Ja existe um cliente cadastrado com este CPF/CNPJ.";
    private readonly IClienteGateway _clienteGateway;

    public AtualizarClienteUseCase(IClienteGateway clienteGateway)
    {
        _clienteGateway = clienteGateway;
    }

    public ClienteOutput? Executar(AtualizarClienteInput input)
    {
        if (_clienteGateway.ExistePorCpfCnpjExcetoId(input.CpfCnpj, input.Id))
        {
            throw new InvalidOperationException(CpfCnpjJaCadastrado);
        }

        var cliente = new Cliente
        {
            Id = input.Id,
            Nome = input.Nome,
            CpfCnpj = input.CpfCnpj,
            Email = input.Email,
            Telefone = input.Telefone
        };

        var clienteAtualizado = _clienteGateway.Atualizar(cliente);

        return clienteAtualizado is null ? null : ClienteOutputMapper.Mapear(clienteAtualizado);
    }
}

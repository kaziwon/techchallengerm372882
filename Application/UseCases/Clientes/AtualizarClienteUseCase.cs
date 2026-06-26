using OficinaMecanica.Api.Application.Exceptions;
using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.Clientes;

public class AtualizarClienteUseCase
{
    private const string CpfCnpjInvalido = "O CPF/CNPJ informado é inválido.";
    private const string CpfCnpjJaCadastrado = "Ja existe um cliente cadastrado com este CPF/CNPJ.";
    private readonly IClienteGateway _clienteGateway;
    private readonly ICpfCnpjValidatorGateway _cpfCnpjValidatorGateway;

    public AtualizarClienteUseCase(IClienteGateway clienteGateway, ICpfCnpjValidatorGateway cpfCnpjValidatorGateway)
    {
        _clienteGateway = clienteGateway;
        _cpfCnpjValidatorGateway = cpfCnpjValidatorGateway;
    }

    public ClienteOutput? Executar(AtualizarClienteInput input)
    {
        var cpfCnpj = ValidarCpfCnpj(input.CpfCnpj);

        if (_clienteGateway.ExistePorCpfCnpjExcetoId(cpfCnpj, input.Id))
        {
            throw new InvalidOperationException(CpfCnpjJaCadastrado);
        }

        var cliente = new Cliente
        {
            Id = input.Id,
            Nome = input.Nome,
            CpfCnpj = cpfCnpj,
            Email = input.Email,
            Telefone = input.Telefone
        };

        var clienteAtualizado = _clienteGateway.Atualizar(cliente);

        return clienteAtualizado is null ? null : ClienteOutputMapper.Mapear(clienteAtualizado);
    }

    private string ValidarCpfCnpj(string cpfCnpj)
    {
        if (!_cpfCnpjValidatorGateway.EhValido(cpfCnpj))
        {
            throw new ValidacaoException(CpfCnpjInvalido);
        }

        return _cpfCnpjValidatorGateway.Normalizar(cpfCnpj);
    }
}

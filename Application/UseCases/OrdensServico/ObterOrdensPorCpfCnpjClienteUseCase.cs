using OficinaMecanica.Api.Application.Exceptions;
using OficinaMecanica.Api.Application.Gateways;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class ObterOrdensPorCpfCnpjClienteUseCase
{
    private const string CpfCnpjInvalido = "O CPF/CNPJ informado é inválido.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;
    private readonly ICpfCnpjValidatorGateway _cpfCnpjValidatorGateway;

    public ObterOrdensPorCpfCnpjClienteUseCase(
        IOrdemServicoGateway ordemServicoGateway,
        ICpfCnpjValidatorGateway cpfCnpjValidatorGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
        _cpfCnpjValidatorGateway = cpfCnpjValidatorGateway;
    }

    public List<OrdemServicoOutput> Executar(string cpfCnpj)
    {
        return _ordemServicoGateway
            .ObterPorCpfCnpjCliente(ValidarCpfCnpj(cpfCnpj))
            .Select(OrdemServicoOutputMapper.Mapear)
            .ToList();
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

using OficinaMecanica.Api.Application.Gateways;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class ObterOrdensPorCpfCnpjClienteUseCase
{
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public ObterOrdensPorCpfCnpjClienteUseCase(IOrdemServicoGateway ordemServicoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
    }

    public List<OrdemServicoOutput> Executar(string cpfCnpj)
    {
        return _ordemServicoGateway
            .ObterPorCpfCnpjCliente(NormalizarCpfCnpj(cpfCnpj))
            .Select(OrdemServicoOutputMapper.Mapear)
            .ToList();
    }

    private static string NormalizarCpfCnpj(string cpfCnpj)
    {
        return new string(cpfCnpj.Where(char.IsDigit).ToArray());
    }
}

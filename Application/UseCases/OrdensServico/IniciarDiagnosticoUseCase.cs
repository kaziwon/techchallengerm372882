using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class IniciarDiagnosticoUseCase
{
    private const string OrdemServicoNaoEstaRecebida = "A ordem de servico nao esta recebida.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public IniciarDiagnosticoUseCase(IOrdemServicoGateway ordemServicoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
    }

    public OrdemServicoOutput? Executar(Guid id)
    {
        var ordemServico = _ordemServicoGateway.ObterPorId(id);

        if (ordemServico is null)
        {
            return null;
        }

        if (ordemServico.Status != StatusOrdemServico.Recebida)
        {
            throw new InvalidOperationException(OrdemServicoNaoEstaRecebida);
        }

        ordemServico.Status = StatusOrdemServico.EmDiagnostico;
        ordemServico.DiagnosticoEm = DateTime.UtcNow;

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Atualizar(ordemServico)!);
    }
}

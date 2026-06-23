using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class FinalizarOrdemServicoUseCase
{
    private const string OrdemServicoNaoEstaEmExecucao = "A ordem de servico nao esta em execucao.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public FinalizarOrdemServicoUseCase(IOrdemServicoGateway ordemServicoGateway)
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

        if (ordemServico.Status != StatusOrdemServico.EmExecucao)
        {
            throw new InvalidOperationException(OrdemServicoNaoEstaEmExecucao);
        }

        ordemServico.Status = StatusOrdemServico.Finalizada;
        ordemServico.FinalizadaEm = DateTime.UtcNow;

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Atualizar(ordemServico)!);
    }
}

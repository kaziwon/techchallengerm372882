using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class EntregarOrdemServicoUseCase
{
    private const string OrdemServicoNaoEstaFinalizada = "A ordem de servico nao esta finalizada.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public EntregarOrdemServicoUseCase(IOrdemServicoGateway ordemServicoGateway)
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

        if (ordemServico.Status != StatusOrdemServico.Finalizada)
        {
            throw new InvalidOperationException(OrdemServicoNaoEstaFinalizada);
        }

        ordemServico.Status = StatusOrdemServico.Entregue;
        ordemServico.EntregueEm = DateTime.UtcNow;

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Atualizar(ordemServico)!);
    }
}

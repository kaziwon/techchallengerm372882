using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class CancelarOrdemServicoUseCase
{
    private const string PecaInsumoNaoEncontrado = "Uma ou mais pecas/insumos informados nao foram encontrados.";
    private const string OrdemServicoNaoPodeSerCancelada = "A ordem de servico nao pode ser cancelada no status atual.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;
    private readonly IPecaInsumoGateway _pecaInsumoGateway;

    public CancelarOrdemServicoUseCase(IOrdemServicoGateway ordemServicoGateway, IPecaInsumoGateway pecaInsumoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
        _pecaInsumoGateway = pecaInsumoGateway;
    }

    public OrdemServicoOutput? Executar(Guid id)
    {
        var ordemServico = _ordemServicoGateway.ObterPorId(id);

        if (ordemServico is null)
        {
            return null;
        }

        if (ordemServico.Status is StatusOrdemServico.Entregue or StatusOrdemServico.Finalizada or StatusOrdemServico.Cancelada)
        {
            throw new InvalidOperationException(OrdemServicoNaoPodeSerCancelada);
        }

        if (ordemServico.Status == StatusOrdemServico.EmExecucao)
        {
            foreach (var itemPecaInsumo in ordemServico.ItensPecaInsumo)
            {
                var pecaInsumo = _pecaInsumoGateway.ObterPorId(itemPecaInsumo.PecaInsumoId);

                if (pecaInsumo is null)
                {
                    throw new InvalidOperationException(PecaInsumoNaoEncontrado);
                }

                pecaInsumo.QuantidadeEstoque += itemPecaInsumo.Quantidade;
                _pecaInsumoGateway.Atualizar(pecaInsumo);
            }
        }

        ordemServico.Status = StatusOrdemServico.Cancelada;

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Atualizar(ordemServico)!);
    }
}

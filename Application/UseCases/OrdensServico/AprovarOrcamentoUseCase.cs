using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class AprovarOrcamentoUseCase
{
    private const string PecaInsumoNaoEncontrado = "Uma ou mais pecas/insumos informados nao foram encontrados.";
    private const string OrcamentoNaoEstaAguardandoAprovacao = "A ordem de servico nao esta aguardando aprovacao do orcamento.";
    private const string EstoqueInsuficiente = "Nao ha estoque suficiente para uma ou mais pecas/insumos.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;
    private readonly IPecaInsumoGateway _pecaInsumoGateway;

    public AprovarOrcamentoUseCase(IOrdemServicoGateway ordemServicoGateway, IPecaInsumoGateway pecaInsumoGateway)
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

        if (ordemServico.Status != StatusOrdemServico.AguardandoAprovacao)
        {
            throw new InvalidOperationException(OrcamentoNaoEstaAguardandoAprovacao);
        }

        foreach (var itemPecaInsumo in ordemServico.ItensPecaInsumo)
        {
            var pecaInsumo = _pecaInsumoGateway.ObterPorId(itemPecaInsumo.PecaInsumoId);

            if (pecaInsumo is null)
            {
                throw new InvalidOperationException(PecaInsumoNaoEncontrado);
            }

            if (pecaInsumo.QuantidadeEstoque < itemPecaInsumo.Quantidade)
            {
                throw new InvalidOperationException(EstoqueInsuficiente);
            }

            pecaInsumo.QuantidadeEstoque -= itemPecaInsumo.Quantidade;
            _pecaInsumoGateway.Atualizar(pecaInsumo);
        }

        ordemServico.Status = StatusOrdemServico.EmExecucao;
        ordemServico.StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Aprovado;
        ordemServico.ExecucaoIniciadaEm = DateTime.UtcNow;
        ordemServico.MotivoRecusaOrcamento = string.Empty;

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Atualizar(ordemServico)!);
    }
}

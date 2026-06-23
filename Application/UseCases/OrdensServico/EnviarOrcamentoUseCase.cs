using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class EnviarOrcamentoUseCase
{
    private const string OrdemServicoNaoEstaEmDiagnostico = "A ordem de servico nao esta em diagnostico.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;
    private readonly IServicoGateway _servicoGateway;
    private readonly IPecaInsumoGateway _pecaInsumoGateway;

    public EnviarOrcamentoUseCase(
        IOrdemServicoGateway ordemServicoGateway,
        IServicoGateway servicoGateway,
        IPecaInsumoGateway pecaInsumoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
        _servicoGateway = servicoGateway;
        _pecaInsumoGateway = pecaInsumoGateway;
    }

    public OrdemServicoOutput? Executar(Guid id, OrdemServicoOrcamentoInput input)
    {
        var ordemServico = _ordemServicoGateway.ObterPorId(id);

        if (ordemServico is null)
        {
            return null;
        }

        if (ordemServico.Status != StatusOrdemServico.EmDiagnostico)
        {
            throw new InvalidOperationException(OrdemServicoNaoEstaEmDiagnostico);
        }

        var itens = OrdemServicoItensBuilder.Montar(
            ordemServico.Id,
            input.ServicoIds,
            input.PecasInsumos,
            _servicoGateway,
            _pecaInsumoGateway);
        var agora = DateTime.UtcNow;

        ordemServico.ItensServico.Clear();
        foreach (var itemServico in itens.ItensServico)
        {
            ordemServico.ItensServico.Add(itemServico);
        }

        ordemServico.ItensPecaInsumo.Clear();
        foreach (var itemPecaInsumo in itens.ItensPecaInsumo)
        {
            ordemServico.ItensPecaInsumo.Add(itemPecaInsumo);
        }

        ordemServico.ValorTotalServicos = itens.ValorTotalServicos;
        ordemServico.ValorTotalPecasInsumos = itens.ValorTotalPecasInsumos;
        ordemServico.ValorTotalOrcamento = itens.ValorTotalOrcamento;
        ordemServico.Status = StatusOrdemServico.AguardandoAprovacao;
        ordemServico.StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Pendente;
        ordemServico.OrcamentoEnviadoEm = agora;
        ordemServico.EnvioOrcamento = $"Orcamento enviado para {ordemServico.Cliente?.Email}";

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Atualizar(ordemServico)!);
    }
}

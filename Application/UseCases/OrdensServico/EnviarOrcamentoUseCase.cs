using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class EnviarOrcamentoUseCase
{
    private const string ServicoNaoEncontrado = "Um ou mais servicos informados nao foram encontrados.";
    private const string PecaInsumoNaoEncontrado = "Uma ou mais pecas/insumos informados nao foram encontrados.";
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

        var itensServico = new List<OrdemServicoItemServico>();
        decimal valorTotalServicos = 0;

        foreach (var servicoId in input.ServicoIds)
        {
            var servico = _servicoGateway.ObterPorId(servicoId);

            if (servico is null)
            {
                throw new InvalidOperationException(ServicoNaoEncontrado);
            }

            itensServico.Add(new OrdemServicoItemServico
            {
                Id = Guid.NewGuid(),
                OrdemServicoId = ordemServico.Id,
                ServicoId = servico.Id,
                NomeServico = servico.Nome,
                PrecoServico = servico.Preco
            });

            valorTotalServicos += servico.Preco;
        }

        var itensPecaInsumo = new List<OrdemServicoItemPecaInsumo>();
        decimal valorTotalPecasInsumos = 0;

        foreach (var itemPeca in input.PecasInsumos)
        {
            var pecaInsumo = _pecaInsumoGateway.ObterPorId(itemPeca.PecaInsumoId);

            if (pecaInsumo is null)
            {
                throw new InvalidOperationException(PecaInsumoNaoEncontrado);
            }

            var subtotal = pecaInsumo.PrecoUnitario * itemPeca.Quantidade;

            itensPecaInsumo.Add(new OrdemServicoItemPecaInsumo
            {
                Id = Guid.NewGuid(),
                OrdemServicoId = ordemServico.Id,
                PecaInsumoId = pecaInsumo.Id,
                NomePecaInsumo = pecaInsumo.Nome,
                PrecoUnitario = pecaInsumo.PrecoUnitario,
                Quantidade = itemPeca.Quantidade,
                Subtotal = subtotal
            });

            valorTotalPecasInsumos += subtotal;
        }

        var agora = DateTime.UtcNow;
        var valorTotalOrcamento = valorTotalServicos + valorTotalPecasInsumos;

        ordemServico.ItensServico.Clear();
        foreach (var itemServico in itensServico)
        {
            ordemServico.ItensServico.Add(itemServico);
        }

        ordemServico.ItensPecaInsumo.Clear();
        foreach (var itemPecaInsumo in itensPecaInsumo)
        {
            ordemServico.ItensPecaInsumo.Add(itemPecaInsumo);
        }

        ordemServico.ValorTotalServicos = valorTotalServicos;
        ordemServico.ValorTotalPecasInsumos = valorTotalPecasInsumos;
        ordemServico.ValorTotalOrcamento = valorTotalOrcamento;
        ordemServico.Status = StatusOrdemServico.AguardandoAprovacao;
        ordemServico.StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Pendente;
        ordemServico.OrcamentoEnviadoEm = agora;
        ordemServico.EnvioOrcamento = $"Orcamento enviado para {ordemServico.Cliente?.Email}";

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Atualizar(ordemServico)!);
    }
}

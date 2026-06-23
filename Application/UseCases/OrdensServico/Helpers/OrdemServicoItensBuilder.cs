using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

internal static class OrdemServicoItensBuilder
{
    private const string ServicoNaoEncontrado = "Um ou mais servicos informados nao foram encontrados.";
    private const string PecaInsumoNaoEncontrado = "Uma ou mais pecas/insumos informados nao foram encontrados.";

    public static OrdemServicoItensResultado Montar(
        Guid ordemServicoId,
        IEnumerable<Guid> servicoIds,
        IEnumerable<OrdemServicoItemPecaInsumoInput> pecasInsumos,
        IServicoGateway servicoGateway,
        IPecaInsumoGateway pecaInsumoGateway)
    {
        var itensServico = new List<OrdemServicoItemServico>();
        decimal valorTotalServicos = 0;

        foreach (var servicoId in servicoIds)
        {
            var servico = servicoGateway.ObterPorId(servicoId);

            if (servico is null)
            {
                throw new InvalidOperationException(ServicoNaoEncontrado);
            }

            itensServico.Add(new OrdemServicoItemServico
            {
                Id = Guid.NewGuid(),
                OrdemServicoId = ordemServicoId,
                ServicoId = servico.Id,
                NomeServico = servico.Nome,
                PrecoServico = servico.Preco
            });

            valorTotalServicos += servico.Preco;
        }

        var itensPecaInsumo = new List<OrdemServicoItemPecaInsumo>();
        decimal valorTotalPecasInsumos = 0;

        foreach (var itemPeca in pecasInsumos)
        {
            var pecaInsumo = pecaInsumoGateway.ObterPorId(itemPeca.PecaInsumoId);

            if (pecaInsumo is null)
            {
                throw new InvalidOperationException(PecaInsumoNaoEncontrado);
            }

            var subtotal = pecaInsumo.PrecoUnitario * itemPeca.Quantidade;

            itensPecaInsumo.Add(new OrdemServicoItemPecaInsumo
            {
                Id = Guid.NewGuid(),
                OrdemServicoId = ordemServicoId,
                PecaInsumoId = pecaInsumo.Id,
                NomePecaInsumo = pecaInsumo.Nome,
                PrecoUnitario = pecaInsumo.PrecoUnitario,
                Quantidade = itemPeca.Quantidade,
                Subtotal = subtotal
            });

            valorTotalPecasInsumos += subtotal;
        }

        return new OrdemServicoItensResultado
        {
            ItensServico = itensServico,
            ItensPecaInsumo = itensPecaInsumo,
            ValorTotalServicos = valorTotalServicos,
            ValorTotalPecasInsumos = valorTotalPecasInsumos,
            ValorTotalOrcamento = valorTotalServicos + valorTotalPecasInsumos
        };
    }
}

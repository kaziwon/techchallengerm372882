using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

internal static class OrdemServicoOutputMapper
{
    public static OrdemServicoOutput Mapear(OrdemServico ordemServico)
    {
        return new OrdemServicoOutput
        {
            Id = ordemServico.Id,
            ClienteId = ordemServico.ClienteId,
            ClienteNome = ordemServico.Cliente?.Nome ?? string.Empty,
            ClienteCpfCnpj = ordemServico.Cliente?.CpfCnpj ?? string.Empty,
            VeiculoId = ordemServico.VeiculoId,
            PlacaVeiculo = ordemServico.Veiculo?.Placa ?? string.Empty,
            ModeloVeiculo = ordemServico.Veiculo?.Modelo ?? string.Empty,
            Status = ordemServico.Status,
            StatusAprovacaoOrcamento = ordemServico.StatusAprovacaoOrcamento,
            ValorTotalServicos = ordemServico.ValorTotalServicos,
            ValorTotalPecasInsumos = ordemServico.ValorTotalPecasInsumos,
            ValorTotalOrcamento = ordemServico.ValorTotalOrcamento,
            EnvioOrcamento = ordemServico.EnvioOrcamento,
            MotivoRecusaOrcamento = ordemServico.MotivoRecusaOrcamento,
            CriadaEm = ordemServico.CriadaEm,
            DiagnosticoEm = ordemServico.DiagnosticoEm,
            OrcamentoEnviadoEm = ordemServico.OrcamentoEnviadoEm,
            ExecucaoIniciadaEm = ordemServico.ExecucaoIniciadaEm,
            FinalizadaEm = ordemServico.FinalizadaEm,
            EntregueEm = ordemServico.EntregueEm,
            ItensServico = ordemServico.ItensServico.Select(item => new OrdemServicoItemServicoOutput
            {
                ServicoId = item.ServicoId,
                NomeServico = item.NomeServico,
                PrecoServico = item.PrecoServico
            }).ToList(),
            ItensPecaInsumo = ordemServico.ItensPecaInsumo.Select(item => new OrdemServicoItemPecaInsumoOutput
            {
                PecaInsumoId = item.PecaInsumoId,
                NomePecaInsumo = item.NomePecaInsumo,
                PrecoUnitario = item.PrecoUnitario,
                Quantidade = item.Quantidade,
                Subtotal = item.Subtotal
            }).ToList()
        };
    }
}

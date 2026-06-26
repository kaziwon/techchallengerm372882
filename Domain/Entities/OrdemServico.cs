namespace OficinaMecanica.Api.Domain.Entities;

public class OrdemServico
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public StatusOrdemServico Status { get; set; }
    public StatusAprovacaoOrcamento StatusAprovacaoOrcamento { get; set; }
    public decimal ValorTotalServicos { get; set; }
    public decimal ValorTotalPecasInsumos { get; set; }
    public decimal ValorTotalOrcamento { get; set; }
    public string EnvioOrcamento { get; set; } = string.Empty;
    public string MotivoRecusaOrcamento { get; set; } = string.Empty;
    public DateTime CriadaEm { get; set; }
    public DateTime? DiagnosticoEm { get; set; }
    public DateTime? OrcamentoEnviadoEm { get; set; }
    public DateTime? ExecucaoIniciadaEm { get; set; }
    public DateTime? FinalizadaEm { get; set; }
    public DateTime? EntregueEm { get; set; }
    public Cliente? Cliente { get; set; }
    public Veiculo? Veiculo { get; set; }
    public ICollection<OrdemServicoItemServico> ItensServico { get; set; } = [];
    public ICollection<OrdemServicoItemPecaInsumo> ItensPecaInsumo { get; set; } = [];
}

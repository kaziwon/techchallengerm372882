using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.InterfaceAdapters.DTOs;

public class OrdemServicoResponseDto
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteCpfCnpj { get; set; } = string.Empty;
    public Guid VeiculoId { get; set; }
    public string PlacaVeiculo { get; set; } = string.Empty;
    public string ModeloVeiculo { get; set; } = string.Empty;
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
    public List<OrdemServicoItemServicoResponseDto> ItensServico { get; set; } = [];
    public List<OrdemServicoItemPecaInsumoResponseDto> ItensPecaInsumo { get; set; } = [];
}

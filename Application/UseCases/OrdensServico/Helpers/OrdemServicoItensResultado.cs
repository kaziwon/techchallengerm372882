using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

internal class OrdemServicoItensResultado
{
    public List<OrdemServicoItemServico> ItensServico { get; set; } = [];
    public List<OrdemServicoItemPecaInsumo> ItensPecaInsumo { get; set; } = [];
    public decimal ValorTotalServicos { get; set; }
    public decimal ValorTotalPecasInsumos { get; set; }
    public decimal ValorTotalOrcamento { get; set; }
}

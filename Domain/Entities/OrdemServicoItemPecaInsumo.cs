namespace OficinaMecanica.Api.Domain.Entities;

public class OrdemServicoItemPecaInsumo
{
    public Guid Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public Guid PecaInsumoId { get; set; }
    public string NomePecaInsumo { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Quantidade { get; set; }
    public decimal Subtotal { get; set; }
    public OrdemServico? OrdemServico { get; set; }
}

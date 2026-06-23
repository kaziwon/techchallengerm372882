namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class OrdemServicoItemPecaInsumoOutput
{
    public Guid PecaInsumoId { get; set; }
    public string NomePecaInsumo { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Quantidade { get; set; }
    public decimal Subtotal { get; set; }
}

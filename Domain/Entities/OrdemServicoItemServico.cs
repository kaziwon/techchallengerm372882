namespace OficinaMecanica.Api.Domain.Entities;

public class OrdemServicoItemServico
{
    public Guid Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public Guid ServicoId { get; set; }
    public string NomeServico { get; set; } = string.Empty;
    public decimal PrecoServico { get; set; }
    public OrdemServico? OrdemServico { get; set; }
}

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class OrdemServicoItemServicoOutput
{
    public Guid ServicoId { get; set; }
    public string NomeServico { get; set; } = string.Empty;
    public decimal PrecoServico { get; set; }
}

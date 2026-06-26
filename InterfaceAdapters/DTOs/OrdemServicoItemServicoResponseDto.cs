namespace OficinaMecanica.Api.InterfaceAdapters.DTOs;

public class OrdemServicoItemServicoResponseDto
{
    public Guid ServicoId { get; set; }
    public string NomeServico { get; set; } = string.Empty;
    public decimal PrecoServico { get; set; }
}

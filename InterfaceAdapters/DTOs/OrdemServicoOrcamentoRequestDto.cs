namespace OficinaMecanica.Api.InterfaceAdapters.DTOs;

public class OrdemServicoOrcamentoRequestDto
{
    public List<Guid> ServicoIds { get; set; } = [];
    public List<OrdemServicoItemPecaInsumoRequestDto> PecasInsumos { get; set; } = [];
}

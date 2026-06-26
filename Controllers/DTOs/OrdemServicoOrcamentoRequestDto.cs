using System.ComponentModel.DataAnnotations;

namespace OficinaMecanica.Api.Controllers.DTOs;

public class OrdemServicoOrcamentoRequestDto
{
    [Required]
    [MinLength(1)]
    public List<Guid> ServicoIds { get; set; } = [];

    public List<OrdemServicoItemPecaInsumoRequestDto> PecasInsumos { get; set; } = [];
}

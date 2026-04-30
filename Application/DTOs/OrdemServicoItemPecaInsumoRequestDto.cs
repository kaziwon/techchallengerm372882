using System.ComponentModel.DataAnnotations;

namespace OficinaMecanica.Api.Application.DTOs;

public class OrdemServicoItemPecaInsumoRequestDto
{
    [Required]
    public Guid PecaInsumoId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantidade { get; set; }
}

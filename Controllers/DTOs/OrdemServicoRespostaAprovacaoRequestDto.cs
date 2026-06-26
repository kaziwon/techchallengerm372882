using System.ComponentModel.DataAnnotations;

namespace OficinaMecanica.Api.Controllers.DTOs;

public class OrdemServicoRespostaAprovacaoRequestDto
{
    [MaxLength(500)]
    public string MotivoRecusa { get; set; } = string.Empty;
}

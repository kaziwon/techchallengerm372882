using System.ComponentModel.DataAnnotations;

namespace OficinaMecanica.Api.Controllers.DTOs;

public class OrdemServicoNotificacaoOrcamentoRequestDto
{
    [Required]
    public bool? Aprovado { get; set; }

    [MaxLength(500)]
    public string MotivoRecusa { get; set; } = string.Empty;
}

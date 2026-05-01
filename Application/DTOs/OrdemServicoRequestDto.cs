using System.ComponentModel.DataAnnotations;
using OficinaMecanica.Api.Application.Validators;

namespace OficinaMecanica.Api.Application.DTOs;

public class OrdemServicoRequestDto
{
    [CpfCnpj]
    public string CpfCnpj { get; set; } = string.Empty;

    [Required]
    public Guid VeiculoId { get; set; }
}

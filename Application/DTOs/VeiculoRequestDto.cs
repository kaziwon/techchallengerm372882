using System.ComponentModel.DataAnnotations;
using OficinaMecanica.Api.Application.Validators;

namespace OficinaMecanica.Api.Application.DTOs;

public class VeiculoRequestDto
{
    [Required]
    public Guid ClienteId { get; set; }

    [PlacaVeiculo]
    public string Placa { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Marca { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Modelo { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int Ano { get; set; }
}

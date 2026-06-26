using System.ComponentModel.DataAnnotations;

namespace OficinaMecanica.Api.Controllers.DTOs;

public class OrdemServicoRequestDto
{
    public string? CpfCnpj { get; set; }

    public Guid? VeiculoId { get; set; }

    public OrdemServicoClienteRequestDto? Cliente { get; set; }
    public OrdemServicoVeiculoRequestDto? Veiculo { get; set; }
    public List<Guid> ServicoIds { get; set; } = [];
    public List<OrdemServicoItemPecaInsumoRequestDto> PecasInsumos { get; set; } = [];

}

public class OrdemServicoClienteRequestDto
{
    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public string CpfCnpj { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Telefone { get; set; } = string.Empty;
}

public class OrdemServicoVeiculoRequestDto
{
    [Required]
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

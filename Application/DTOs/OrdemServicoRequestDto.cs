using System.ComponentModel.DataAnnotations;
using OficinaMecanica.Api.Application.Validators;

namespace OficinaMecanica.Api.Application.DTOs;

public class OrdemServicoRequestDto : IValidatableObject
{
    public string? CpfCnpj { get; set; }

    public Guid? VeiculoId { get; set; }

    public OrdemServicoClienteRequestDto? Cliente { get; set; }
    public OrdemServicoVeiculoRequestDto? Veiculo { get; set; }
    public List<Guid> ServicoIds { get; set; } = [];
    public List<OrdemServicoItemPecaInsumoRequestDto> PecasInsumos { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var informouCpfCnpj = !string.IsNullOrWhiteSpace(CpfCnpj);
        var informouVeiculoId = VeiculoId.HasValue;
        var informouCliente = Cliente is not null;
        var informouVeiculo = Veiculo is not null;

        var modoExistente = informouCpfCnpj && informouVeiculoId && !informouCliente && !informouVeiculo;
        var modoCadastroCompleto = !informouCpfCnpj && !informouVeiculoId && informouCliente && informouVeiculo;

        if (modoExistente)
        {
            var resultadoCpfCnpj = new CpfCnpjAttribute().GetValidationResult(CpfCnpj, validationContext);

            if (resultadoCpfCnpj != ValidationResult.Success)
            {
                yield return new ValidationResult(resultadoCpfCnpj?.ErrorMessage, [nameof(CpfCnpj)]);
            }

            yield break;
        }

        if (modoCadastroCompleto)
        {
            yield break;
        }

        yield return new ValidationResult(
            "Informe CpfCnpj e VeiculoId para usar cadastro existente, ou Cliente e Veiculo para criar cadastro completo.",
            [nameof(CpfCnpj), nameof(VeiculoId), nameof(Cliente), nameof(Veiculo)]);
    }
}

public class OrdemServicoClienteRequestDto
{
    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [CpfCnpj]
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

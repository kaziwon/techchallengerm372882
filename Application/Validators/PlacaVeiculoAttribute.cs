using BrazilianDocuments.Validators;
using System.ComponentModel.DataAnnotations;

namespace OficinaMecanica.Api.Application.Validators;

public class PlacaVeiculoAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string placa || string.IsNullOrWhiteSpace(placa))
        {
            return new ValidationResult("O campo placa é obrigatório.");
        }

        var placaLimpa = PlacaVeicularValidator.ClearCode(placa);

        if (!PlacaVeicularValidator.IsValid(placaLimpa))
        {
            return new ValidationResult("A placa informada é inválida.");
        }

        return ValidationResult.Success;
    }
}

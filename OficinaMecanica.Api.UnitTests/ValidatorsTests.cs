using System.ComponentModel.DataAnnotations;
using OficinaMecanica.Api.Application.Validators;

namespace OficinaMecanica.Api.UnitTests;

public class ValidatorsTests
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    [InlineData("11222333000181")]
    [InlineData("11.222.333/0001-81")]
    public void CpfCnpjAttribute_DeveAceitarDocumentosValidos(string valor)
    {
        var attribute = new CpfCnpjAttribute();

        var result = attribute.GetValidationResult(valor, new ValidationContext(new object()));

        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("11111111111")]
    [InlineData("12345678901")]
    [InlineData("11222333000100")]
    public void CpfCnpjAttribute_DeveRejeitarDocumentosInvalidos(string valor)
    {
        var attribute = new CpfCnpjAttribute();

        var result = attribute.GetValidationResult(valor, new ValidationContext(new object()));

        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("BRA2E19")]
    [InlineData("ABC-1234")]
    public void PlacaVeiculoAttribute_DeveAceitarPlacasValidas(string valor)
    {
        var attribute = new PlacaVeiculoAttribute();

        var result = attribute.GetValidationResult(valor, new ValidationContext(new object()));

        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("AAA000")]
    [InlineData("1234567")]
    public void PlacaVeiculoAttribute_DeveRejeitarPlacasInvalidas(string valor)
    {
        var attribute = new PlacaVeiculoAttribute();

        var result = attribute.GetValidationResult(valor, new ValidationContext(new object()));

        Assert.NotEqual(ValidationResult.Success, result);
    }
}

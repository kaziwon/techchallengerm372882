using OficinaMecanica.Api.Infrastructure.Sources;
using OficinaMecanica.Api.InterfaceAdapters.Gateways;

namespace OficinaMecanica.Api.UnitTests;

public class ValidatorsTests
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    [InlineData("11222333000181")]
    [InlineData("11.222.333/0001-81")]
    public void CpfCnpjValidatorGateway_DeveAceitarDocumentosValidos(string valor)
    {
        var validator = new CpfCnpjValidatorGateway(new CpfCnpjValidatorSource());

        Assert.True(validator.EhValido(valor));
    }

    [Theory]
    [InlineData("")]
    [InlineData("11111111111")]
    [InlineData("12345678901")]
    [InlineData("11222333000100")]
    public void CpfCnpjValidatorGateway_DeveRejeitarDocumentosInvalidos(string valor)
    {
        var validator = new CpfCnpjValidatorGateway(new CpfCnpjValidatorSource());

        Assert.False(validator.EhValido(valor));
    }

    [Theory]
    [InlineData("BRA2E19")]
    [InlineData("ABC-1234")]
    public void PlacaVeiculoValidatorGateway_DeveAceitarPlacasValidas(string valor)
    {
        var validator = new PlacaVeiculoValidatorGateway(new BrazilianDocumentsPlacaVeiculoValidatorSource());

        Assert.True(validator.EhValida(valor));
    }

    [Theory]
    [InlineData("")]
    [InlineData("AAA000")]
    [InlineData("1234567")]
    public void PlacaVeiculoValidatorGateway_DeveRejeitarPlacasInvalidas(string valor)
    {
        var validator = new PlacaVeiculoValidatorGateway(new BrazilianDocumentsPlacaVeiculoValidatorSource());

        Assert.False(validator.EhValida(valor));
    }
}

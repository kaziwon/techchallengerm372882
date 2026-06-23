using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.Veiculos;

internal static class VeiculoOutputMapper
{
    public static VeiculoOutput Mapear(Veiculo veiculo)
    {
        return new VeiculoOutput
        {
            Id = veiculo.Id,
            ClienteId = veiculo.ClienteId,
            Placa = veiculo.Placa,
            Marca = veiculo.Marca,
            Modelo = veiculo.Modelo,
            Ano = veiculo.Ano
        };
    }
}

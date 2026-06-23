using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.PecasInsumos;

internal static class PecaInsumoOutputMapper
{
    public static PecaInsumoOutput Mapear(PecaInsumo pecaInsumo)
    {
        return new PecaInsumoOutput
        {
            Id = pecaInsumo.Id,
            Nome = pecaInsumo.Nome,
            Descricao = pecaInsumo.Descricao,
            PrecoUnitario = pecaInsumo.PrecoUnitario,
            QuantidadeEstoque = pecaInsumo.QuantidadeEstoque
        };
    }
}

using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.PecasInsumos;

public class AtualizarPecaInsumoUseCase
{
    private const string NomeJaCadastrado = "Ja existe uma peca ou insumo cadastrado com este nome.";
    private readonly IPecaInsumoGateway _pecaInsumoGateway;

    public AtualizarPecaInsumoUseCase(IPecaInsumoGateway pecaInsumoGateway)
    {
        _pecaInsumoGateway = pecaInsumoGateway;
    }

    public PecaInsumoOutput? Executar(Guid id, PecaInsumoInput input)
    {
        if (_pecaInsumoGateway.ExistePorNomeExcetoId(input.Nome, id))
        {
            throw new InvalidOperationException(NomeJaCadastrado);
        }

        var pecaInsumo = new PecaInsumo
        {
            Id = id,
            Nome = input.Nome,
            Descricao = input.Descricao,
            PrecoUnitario = input.PrecoUnitario,
            QuantidadeEstoque = input.QuantidadeEstoque
        };

        var pecaInsumoAtualizada = _pecaInsumoGateway.Atualizar(pecaInsumo);

        return pecaInsumoAtualizada is null ? null : PecaInsumoOutputMapper.Mapear(pecaInsumoAtualizada);
    }
}

using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.PecasInsumos;

public class CriarPecaInsumoUseCase
{
    private const string NomeJaCadastrado = "Ja existe uma peca ou insumo cadastrado com este nome.";
    private readonly IPecaInsumoGateway _pecaInsumoGateway;

    public CriarPecaInsumoUseCase(IPecaInsumoGateway pecaInsumoGateway)
    {
        _pecaInsumoGateway = pecaInsumoGateway;
    }

    public PecaInsumoOutput Executar(PecaInsumoInput input)
    {
        if (_pecaInsumoGateway.ExistePorNome(input.Nome))
        {
            throw new InvalidOperationException(NomeJaCadastrado);
        }

        var pecaInsumo = new PecaInsumo
        {
            Id = Guid.NewGuid(),
            Nome = input.Nome,
            Descricao = input.Descricao,
            PrecoUnitario = input.PrecoUnitario,
            QuantidadeEstoque = input.QuantidadeEstoque
        };

        return PecaInsumoOutputMapper.Mapear(_pecaInsumoGateway.Adicionar(pecaInsumo));
    }
}

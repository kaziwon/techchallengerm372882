using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.PecasInsumos;

public record PecaInsumoInput(string Nome, string Descricao, decimal PrecoUnitario, int QuantidadeEstoque);

public class PecaInsumoOutput
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int QuantidadeEstoque { get; set; }
}

public class ObterTodasPecasInsumosUseCase
{
    private readonly IPecaInsumoGateway _pecaInsumoGateway;

    public ObterTodasPecasInsumosUseCase(IPecaInsumoGateway pecaInsumoGateway)
    {
        _pecaInsumoGateway = pecaInsumoGateway;
    }

    public List<PecaInsumoOutput> Executar()
    {
        return _pecaInsumoGateway.ObterTodos().Select(PecaInsumoOutputMapper.Mapear).ToList();
    }
}

public class ObterPecaInsumoPorIdUseCase
{
    private readonly IPecaInsumoGateway _pecaInsumoGateway;

    public ObterPecaInsumoPorIdUseCase(IPecaInsumoGateway pecaInsumoGateway)
    {
        _pecaInsumoGateway = pecaInsumoGateway;
    }

    public PecaInsumoOutput? Executar(Guid id)
    {
        var pecaInsumo = _pecaInsumoGateway.ObterPorId(id);

        return pecaInsumo is null ? null : PecaInsumoOutputMapper.Mapear(pecaInsumo);
    }
}

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

public class RemoverPecaInsumoUseCase
{
    private readonly IPecaInsumoGateway _pecaInsumoGateway;

    public RemoverPecaInsumoUseCase(IPecaInsumoGateway pecaInsumoGateway)
    {
        _pecaInsumoGateway = pecaInsumoGateway;
    }

    public bool Executar(Guid id)
    {
        return _pecaInsumoGateway.Remover(id);
    }
}

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

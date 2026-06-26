using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.UseCases.PecasInsumos;

namespace OficinaMecanica.Api.InterfaceAdapters.Controllers;

public class PecasCleanController
{
    private readonly ObterTodasPecasInsumosUseCase _obterTodasPecasInsumosUseCase;
    private readonly ObterPecaInsumoPorIdUseCase _obterPecaInsumoPorIdUseCase;
    private readonly CriarPecaInsumoUseCase _criarPecaInsumoUseCase;
    private readonly AtualizarPecaInsumoUseCase _atualizarPecaInsumoUseCase;
    private readonly RemoverPecaInsumoUseCase _removerPecaInsumoUseCase;

    public PecasCleanController(
        ObterTodasPecasInsumosUseCase obterTodasPecasInsumosUseCase,
        ObterPecaInsumoPorIdUseCase obterPecaInsumoPorIdUseCase,
        CriarPecaInsumoUseCase criarPecaInsumoUseCase,
        AtualizarPecaInsumoUseCase atualizarPecaInsumoUseCase,
        RemoverPecaInsumoUseCase removerPecaInsumoUseCase)
    {
        _obterTodasPecasInsumosUseCase = obterTodasPecasInsumosUseCase;
        _obterPecaInsumoPorIdUseCase = obterPecaInsumoPorIdUseCase;
        _criarPecaInsumoUseCase = criarPecaInsumoUseCase;
        _atualizarPecaInsumoUseCase = atualizarPecaInsumoUseCase;
        _removerPecaInsumoUseCase = removerPecaInsumoUseCase;
    }

    public List<PecaInsumoResponseDto> ObterTodas()
    {
        return _obterTodasPecasInsumosUseCase.Executar().Select(MapearResponse).ToList();
    }

    public PecaInsumoResponseDto? ObterPorId(Guid id)
    {
        var pecaInsumo = _obterPecaInsumoPorIdUseCase.Executar(id);

        return pecaInsumo is null ? null : MapearResponse(pecaInsumo);
    }

    public PecaInsumoResponseDto Criar(PecaInsumoRequestDto pecaInsumoRequestDto)
    {
        var input = new PecaInsumoInput(
            pecaInsumoRequestDto.Nome,
            pecaInsumoRequestDto.Descricao,
            pecaInsumoRequestDto.PrecoUnitario,
            pecaInsumoRequestDto.QuantidadeEstoque);

        return MapearResponse(_criarPecaInsumoUseCase.Executar(input));
    }

    public PecaInsumoResponseDto? Atualizar(Guid id, PecaInsumoRequestDto pecaInsumoRequestDto)
    {
        var input = new PecaInsumoInput(
            pecaInsumoRequestDto.Nome,
            pecaInsumoRequestDto.Descricao,
            pecaInsumoRequestDto.PrecoUnitario,
            pecaInsumoRequestDto.QuantidadeEstoque);
        var pecaInsumo = _atualizarPecaInsumoUseCase.Executar(id, input);

        return pecaInsumo is null ? null : MapearResponse(pecaInsumo);
    }

    public bool Remover(Guid id)
    {
        return _removerPecaInsumoUseCase.Executar(id);
    }

    private static PecaInsumoResponseDto MapearResponse(PecaInsumoOutput pecaInsumo)
    {
        return new PecaInsumoResponseDto
        {
            Id = pecaInsumo.Id,
            Nome = pecaInsumo.Nome,
            Descricao = pecaInsumo.Descricao,
            PrecoUnitario = pecaInsumo.PrecoUnitario,
            QuantidadeEstoque = pecaInsumo.QuantidadeEstoque
        };
    }
}

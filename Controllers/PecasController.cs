using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.UseCases.PecasInsumos;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PecasController : ControllerBase
{
    private readonly ObterTodasPecasInsumosUseCase _obterTodasPecasInsumosUseCase;
    private readonly ObterPecaInsumoPorIdUseCase _obterPecaInsumoPorIdUseCase;
    private readonly CriarPecaInsumoUseCase _criarPecaInsumoUseCase;
    private readonly AtualizarPecaInsumoUseCase _atualizarPecaInsumoUseCase;
    private readonly RemoverPecaInsumoUseCase _removerPecaInsumoUseCase;

    public PecasController(
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

    [HttpGet]
    [ProducesResponseType(typeof(List<PecaInsumoResponseDto>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(_obterTodasPecasInsumosUseCase.Executar().Select(MapearResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PecaInsumoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var pecaInsumo = _obterPecaInsumoPorIdUseCase.Executar(id);

        return pecaInsumo is null ? NotFound() : Ok(MapearResponse(pecaInsumo));
    }

    [HttpPost]
    [ProducesResponseType(typeof(PecaInsumoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Post([FromBody] PecaInsumoRequestDto pecaInsumoRequestDto)
    {
        try
        {
            var input = new PecaInsumoInput(
                pecaInsumoRequestDto.Nome,
                pecaInsumoRequestDto.Descricao,
                pecaInsumoRequestDto.PrecoUnitario,
                pecaInsumoRequestDto.QuantidadeEstoque);
            var pecaInsumo = MapearResponse(_criarPecaInsumoUseCase.Executar(input));
            return CreatedAtAction(nameof(GetById), new { id = pecaInsumo.Id }, pecaInsumo);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PecaInsumoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Put(Guid id, [FromBody] PecaInsumoRequestDto pecaInsumoRequestDto)
    {
        try
        {
            var input = new PecaInsumoInput(
                pecaInsumoRequestDto.Nome,
                pecaInsumoRequestDto.Descricao,
                pecaInsumoRequestDto.PrecoUnitario,
                pecaInsumoRequestDto.QuantidadeEstoque);
            var pecaInsumo = _atualizarPecaInsumoUseCase.Executar(id, input);

            return pecaInsumo is null ? NotFound() : Ok(MapearResponse(pecaInsumo));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        var removido = _removerPecaInsumoUseCase.Executar(id);

        return removido ? NoContent() : NotFound();
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

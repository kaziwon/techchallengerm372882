using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.UseCases.Servicos;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicosController : ControllerBase
{
    private readonly ObterTodosServicosUseCase _obterTodosServicosUseCase;
    private readonly ObterServicoPorIdUseCase _obterServicoPorIdUseCase;
    private readonly CriarServicoUseCase _criarServicoUseCase;
    private readonly AtualizarServicoUseCase _atualizarServicoUseCase;
    private readonly RemoverServicoUseCase _removerServicoUseCase;

    public ServicosController(
        ObterTodosServicosUseCase obterTodosServicosUseCase,
        ObterServicoPorIdUseCase obterServicoPorIdUseCase,
        CriarServicoUseCase criarServicoUseCase,
        AtualizarServicoUseCase atualizarServicoUseCase,
        RemoverServicoUseCase removerServicoUseCase)
    {
        _obterTodosServicosUseCase = obterTodosServicosUseCase;
        _obterServicoPorIdUseCase = obterServicoPorIdUseCase;
        _criarServicoUseCase = criarServicoUseCase;
        _atualizarServicoUseCase = atualizarServicoUseCase;
        _removerServicoUseCase = removerServicoUseCase;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ServicoResponseDto>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(_obterTodosServicosUseCase.Executar().Select(MapearResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ServicoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var servico = _obterServicoPorIdUseCase.Executar(id);

        return servico is null ? NotFound() : Ok(MapearResponse(servico));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ServicoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Post([FromBody] ServicoRequestDto servicoRequestDto)
    {
        try
        {
            var input = new ServicoInput(servicoRequestDto.Nome, servicoRequestDto.Descricao, servicoRequestDto.Preco);
            var servico = MapearResponse(_criarServicoUseCase.Executar(input));
            return CreatedAtAction(nameof(GetById), new { id = servico.Id }, servico);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ServicoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Put(Guid id, [FromBody] ServicoRequestDto servicoRequestDto)
    {
        try
        {
            var input = new ServicoInput(servicoRequestDto.Nome, servicoRequestDto.Descricao, servicoRequestDto.Preco);
            var servico = _atualizarServicoUseCase.Executar(id, input);

            return servico is null ? NotFound() : Ok(MapearResponse(servico));
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
        var removido = _removerServicoUseCase.Executar(id);

        return removido ? NoContent() : NotFound();
    }

    private static ServicoResponseDto MapearResponse(ServicoOutput servico)
    {
        return new ServicoResponseDto
        {
            Id = servico.Id,
            Nome = servico.Nome,
            Descricao = servico.Descricao,
            Preco = servico.Preco
        };
    }
}

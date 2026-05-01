using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.Interfaces;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicosController : ControllerBase
{
    private readonly IServicoService _servicoService;

    public ServicosController(IServicoService servicoService)
    {
        _servicoService = servicoService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ServicoResponseDto>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(_servicoService.ObterTodos());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ServicoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var servico = _servicoService.ObterPorId(id);

        return servico is null ? NotFound() : Ok(servico);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ServicoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Post([FromBody] ServicoRequestDto servicoRequestDto)
    {
        try
        {
            var servico = _servicoService.Adicionar(servicoRequestDto);
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
            var servico = _servicoService.Atualizar(id, servicoRequestDto);

            return servico is null ? NotFound() : Ok(servico);
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
        var removido = _servicoService.Remover(id);

        return removido ? NoContent() : NotFound();
    }
}

using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.Interfaces;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PecasController : ControllerBase
{
    private readonly IPecaInsumoService _pecaInsumoService;

    public PecasController(IPecaInsumoService pecaInsumoService)
    {
        _pecaInsumoService = pecaInsumoService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<PecaInsumoResponseDto>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(_pecaInsumoService.ObterTodos());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PecaInsumoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var pecaInsumo = _pecaInsumoService.ObterPorId(id);

        return pecaInsumo is null ? NotFound() : Ok(pecaInsumo);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PecaInsumoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Post([FromBody] PecaInsumoRequestDto pecaInsumoRequestDto)
    {
        try
        {
            var pecaInsumo = _pecaInsumoService.Adicionar(pecaInsumoRequestDto);
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
            var pecaInsumo = _pecaInsumoService.Atualizar(id, pecaInsumoRequestDto);

            return pecaInsumo is null ? NotFound() : Ok(pecaInsumo);
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
        var removido = _pecaInsumoService.Remover(id);

        return removido ? NoContent() : NotFound();
    }
}

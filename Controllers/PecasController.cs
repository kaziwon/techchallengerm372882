using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.InterfaceAdapters.Controllers;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PecasController : ControllerBase
{
    private readonly PecasCleanController _pecasCleanController;

    public PecasController(PecasCleanController pecasCleanController)
    {
        _pecasCleanController = pecasCleanController;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<PecaInsumoResponseDto>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(_pecasCleanController.ObterTodas());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PecaInsumoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var pecaInsumo = _pecasCleanController.ObterPorId(id);

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
            var pecaInsumo = _pecasCleanController.Criar(pecaInsumoRequestDto);
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
            var pecaInsumo = _pecasCleanController.Atualizar(id, pecaInsumoRequestDto);

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
        var removido = _pecasCleanController.Remover(id);

        return removido ? NoContent() : NotFound();
    }
}

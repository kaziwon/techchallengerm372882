using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.Interfaces;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VeiculosController : ControllerBase
{
    private readonly IVeiculoService _veiculoService;

    public VeiculosController(IVeiculoService veiculoService)
    {
        _veiculoService = veiculoService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<VeiculoResponseDto>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(_veiculoService.ObterTodos());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VeiculoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var veiculo = _veiculoService.ObterPorId(id);

        return veiculo is null ? NotFound() : Ok(veiculo);
    }

    [HttpPost]
    [ProducesResponseType(typeof(VeiculoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Post([FromBody] VeiculoRequestDto veiculoRequestDto)
    {
        try
        {
            var veiculo = _veiculoService.Adicionar(veiculoRequestDto);
            return CreatedAtAction(nameof(GetById), new { id = veiculo.Id }, veiculo);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(VeiculoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Put(Guid id, [FromBody] VeiculoRequestDto veiculoRequestDto)
    {
        try
        {
            var veiculo = _veiculoService.Atualizar(id, veiculoRequestDto);

            return veiculo is null ? NotFound() : Ok(veiculo);
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
        var removido = _veiculoService.Remover(id);

        return removido ? NoContent() : NotFound();
    }
}

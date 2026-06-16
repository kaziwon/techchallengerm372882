using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.UseCases.Veiculos;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VeiculosController : ControllerBase
{
    private readonly ObterTodosVeiculosUseCase _obterTodosVeiculosUseCase;
    private readonly ObterVeiculoPorIdUseCase _obterVeiculoPorIdUseCase;
    private readonly CriarVeiculoUseCase _criarVeiculoUseCase;
    private readonly AtualizarVeiculoUseCase _atualizarVeiculoUseCase;
    private readonly RemoverVeiculoUseCase _removerVeiculoUseCase;

    public VeiculosController(
        ObterTodosVeiculosUseCase obterTodosVeiculosUseCase,
        ObterVeiculoPorIdUseCase obterVeiculoPorIdUseCase,
        CriarVeiculoUseCase criarVeiculoUseCase,
        AtualizarVeiculoUseCase atualizarVeiculoUseCase,
        RemoverVeiculoUseCase removerVeiculoUseCase)
    {
        _obterTodosVeiculosUseCase = obterTodosVeiculosUseCase;
        _obterVeiculoPorIdUseCase = obterVeiculoPorIdUseCase;
        _criarVeiculoUseCase = criarVeiculoUseCase;
        _atualizarVeiculoUseCase = atualizarVeiculoUseCase;
        _removerVeiculoUseCase = removerVeiculoUseCase;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<VeiculoResponseDto>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(_obterTodosVeiculosUseCase.Executar().Select(MapearResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VeiculoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var veiculo = _obterVeiculoPorIdUseCase.Executar(id);

        return veiculo is null ? NotFound() : Ok(MapearResponse(veiculo));
    }

    [HttpPost]
    [ProducesResponseType(typeof(VeiculoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Post([FromBody] VeiculoRequestDto veiculoRequestDto)
    {
        try
        {
            var input = new VeiculoInput(
                veiculoRequestDto.ClienteId,
                veiculoRequestDto.Placa,
                veiculoRequestDto.Marca,
                veiculoRequestDto.Modelo,
                veiculoRequestDto.Ano);
            var veiculo = MapearResponse(_criarVeiculoUseCase.Executar(input));
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
            var input = new VeiculoInput(
                veiculoRequestDto.ClienteId,
                veiculoRequestDto.Placa,
                veiculoRequestDto.Marca,
                veiculoRequestDto.Modelo,
                veiculoRequestDto.Ano);
            var veiculo = _atualizarVeiculoUseCase.Executar(id, input);

            return veiculo is null ? NotFound() : Ok(MapearResponse(veiculo));
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
        var removido = _removerVeiculoUseCase.Executar(id);

        return removido ? NoContent() : NotFound();
    }

    private static VeiculoResponseDto MapearResponse(VeiculoOutput veiculo)
    {
        return new VeiculoResponseDto
        {
            Id = veiculo.Id,
            ClienteId = veiculo.ClienteId,
            Placa = veiculo.Placa,
            Marca = veiculo.Marca,
            Modelo = veiculo.Modelo,
            Ano = veiculo.Ano
        };
    }
}

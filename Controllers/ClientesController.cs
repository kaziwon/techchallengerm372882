using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.UseCases.Clientes;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly ObterTodosClientesUseCase _obterTodosClientesUseCase;
    private readonly ObterClientePorIdUseCase _obterClientePorIdUseCase;
    private readonly CriarClienteUseCase _criarClienteUseCase;
    private readonly AtualizarClienteUseCase _atualizarClienteUseCase;
    private readonly RemoverClienteUseCase _removerClienteUseCase;

    public ClientesController(
        ObterTodosClientesUseCase obterTodosClientesUseCase,
        ObterClientePorIdUseCase obterClientePorIdUseCase,
        CriarClienteUseCase criarClienteUseCase,
        AtualizarClienteUseCase atualizarClienteUseCase,
        RemoverClienteUseCase removerClienteUseCase)
    {
        _obterTodosClientesUseCase = obterTodosClientesUseCase;
        _obterClientePorIdUseCase = obterClientePorIdUseCase;
        _criarClienteUseCase = criarClienteUseCase;
        _atualizarClienteUseCase = atualizarClienteUseCase;
        _removerClienteUseCase = removerClienteUseCase;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ClienteResponseDto>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var clientes = _obterTodosClientesUseCase
            .Executar()
            .Select(MapearResponse)
            .ToList();

        return Ok(clientes);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var cliente = _obterClientePorIdUseCase.Executar(id);

        if (cliente is null)
        {
            return NotFound();
        }

        return Ok(MapearResponse(cliente));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Post([FromBody] ClienteRequestDto clienteRequestDto)
    {
        try
        {
            var input = new CriarClienteInput(
                clienteRequestDto.Nome,
                clienteRequestDto.CpfCnpj,
                clienteRequestDto.Email,
                clienteRequestDto.Telefone);
            var clienteAdicionado = MapearResponse(_criarClienteUseCase.Executar(input));

            return CreatedAtAction(nameof(GetById), new { id = clienteAdicionado.Id }, clienteAdicionado);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Put(Guid id, [FromBody] ClienteRequestDto clienteRequestDto)
    {
        try
        {
            var input = new AtualizarClienteInput(
                id,
                clienteRequestDto.Nome,
                clienteRequestDto.CpfCnpj,
                clienteRequestDto.Email,
                clienteRequestDto.Telefone);
            var clienteAtualizado = _atualizarClienteUseCase.Executar(input);

            if (clienteAtualizado is null)
            {
                return NotFound();
            }

            return Ok(MapearResponse(clienteAtualizado));
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
        var removido = _removerClienteUseCase.Executar(id);

        if (!removido)
        {
            return NotFound();
        }

        return NoContent();
    }

    private static ClienteResponseDto MapearResponse(ClienteOutput cliente)
    {
        return new ClienteResponseDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            CpfCnpj = cliente.CpfCnpj,
            Email = cliente.Email,
            Telefone = cliente.Telefone
        };
    }
}

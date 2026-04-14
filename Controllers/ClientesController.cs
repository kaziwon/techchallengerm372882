using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.Interfaces;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var clientes = _clienteService.ObterTodos();

        return Ok(clientes);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var cliente = _clienteService.ObterPorId(id);

        if (cliente is null)
        {
            return NotFound();
        }

        return Ok(cliente);
    }

    [HttpPost]
    public IActionResult Post([FromBody] ClienteRequestDto clienteRequestDto)
    {
        var clienteAdicionado = _clienteService.Adicionar(clienteRequestDto);

        return CreatedAtAction(nameof(GetById), new { id = clienteAdicionado.Id }, clienteAdicionado);
    }

    [HttpPut("{id:guid}")]
    public IActionResult Put(Guid id, [FromBody] ClienteRequestDto clienteRequestDto)
    {
        var clienteAtualizado = _clienteService.Atualizar(id, clienteRequestDto);

        if (clienteAtualizado is null)
        {
            return NotFound();
        }

        return Ok(clienteAtualizado);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var removido = _clienteService.Remover(id);

        if (!removido)
        {
            return NotFound();
        }

        return NoContent();
    }
}

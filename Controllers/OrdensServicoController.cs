using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.InterfaceAdapters.Controllers;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdensServicoController : ControllerBase
{
    private readonly OrdensServicoCleanController _ordensServicoCleanController;

    public OrdensServicoController(OrdensServicoCleanController ordensServicoCleanController)
    {
        _ordensServicoCleanController = ordensServicoCleanController;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<OrdemServicoResponseDto>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(_ordensServicoCleanController.ObterTodas());
    }

    [HttpGet("tempo-medio-execucao")]
    [ProducesResponseType(typeof(TempoMedioExecucaoResponseDto), StatusCodes.Status200OK)]
    public IActionResult GetTempoMedioExecucao()
    {
        return Ok(_ordensServicoCleanController.ObterTempoMedioExecucao());
    }

    [HttpGet("cliente/{cpfCnpj}")]
    [ProducesResponseType(typeof(List<OrdemServicoResponseDto>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public IActionResult GetByCpfCnpjCliente(string cpfCnpj)
    {
        return Ok(_ordensServicoCleanController.ObterPorCpfCnpjCliente(cpfCnpj));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrdemServicoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var ordemServico = _ordensServicoCleanController.ObterPorId(id);

        return ordemServico is null ? NotFound() : Ok(ordemServico);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrdemServicoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Post([FromBody] OrdemServicoRequestDto ordemServicoRequestDto)
    {
        try
        {
            var ordemServico = _ordensServicoCleanController.Criar(ordemServicoRequestDto);
            return CreatedAtAction(nameof(GetById), new { id = ordemServico.Id }, ordemServico);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/iniciar-diagnostico")]
    [ProducesResponseType(typeof(OrdemServicoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult IniciarDiagnostico(Guid id)
    {
        try
        {
            var ordemServico = _ordensServicoCleanController.IniciarDiagnostico(id);
            return ordemServico is null ? NotFound() : Ok(ordemServico);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/enviar-orcamento")]
    [ProducesResponseType(typeof(OrdemServicoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult EnviarOrcamento(Guid id, [FromBody] OrdemServicoOrcamentoRequestDto requestDto)
    {
        try
        {
            var ordemServico = _ordensServicoCleanController.EnviarOrcamento(id, requestDto);
            return ordemServico is null ? NotFound() : Ok(ordemServico);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/aprovar-orcamento")]
    [ProducesResponseType(typeof(OrdemServicoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult AprovarOrcamento(Guid id)
    {
        try
        {
            var ordemServico = _ordensServicoCleanController.AprovarOrcamento(id);
            return ordemServico is null ? NotFound() : Ok(ordemServico);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/recusar-orcamento")]
    [ProducesResponseType(typeof(OrdemServicoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult RecusarOrcamento(Guid id, [FromBody] OrdemServicoRespostaAprovacaoRequestDto requestDto)
    {
        try
        {
            var ordemServico = _ordensServicoCleanController.RecusarOrcamento(id, requestDto);
            return ordemServico is null ? NotFound() : Ok(ordemServico);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancelar")]
    [ProducesResponseType(typeof(OrdemServicoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Cancelar(Guid id)
    {
        try
        {
            var ordemServico = _ordensServicoCleanController.Cancelar(id);
            return ordemServico is null ? NotFound() : Ok(ordemServico);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/finalizar")]
    [ProducesResponseType(typeof(OrdemServicoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Finalizar(Guid id)
    {
        try
        {
            var ordemServico = _ordensServicoCleanController.Finalizar(id);
            return ordemServico is null ? NotFound() : Ok(ordemServico);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/entregar")]
    [ProducesResponseType(typeof(OrdemServicoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Entregar(Guid id)
    {
        try
        {
            var ordemServico = _ordensServicoCleanController.Entregar(id);
            return ordemServico is null ? NotFound() : Ok(ordemServico);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

}

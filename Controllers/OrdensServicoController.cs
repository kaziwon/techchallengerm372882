using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Api.Application.Exceptions;
using OficinaMecanica.Api.Application.UseCases.OrdensServico;
using OficinaMecanica.Api.Controllers.Mappers;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.InterfaceAdapters.Controllers;
using OficinaMecanica.Api.InterfaceAdapters.DTOs;
using HttpDtos = OficinaMecanica.Api.Controllers.DTOs;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdensServicoController : ControllerBase
{
    private readonly OrdensServicoCleanController _ordensServicoCleanController;
    private readonly ILogger<OrdensServicoController> _logger;

    public OrdensServicoController(
        OrdensServicoCleanController ordensServicoCleanController,
        ILogger<OrdensServicoController> logger)
    {
        _ordensServicoCleanController = ordensServicoCleanController;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<OrdemServicoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Get([FromQuery] string? status, [FromQuery] string? ordenacao)
    {
        try
        {
            return Ok(_ordensServicoCleanController.ObterTodas(
                MapearStatus(status),
                MapearOrdenacao(ordenacao)));
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
        try
        {
            return Ok(_ordensServicoCleanController.ObterPorCpfCnpjCliente(cpfCnpj));
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
    public IActionResult Post([FromBody] HttpDtos.OrdemServicoRequestDto ordemServicoRequestDto)
    {
        try
        {
            var ordemServico = _ordensServicoCleanController.Criar(ordemServicoRequestDto.ParaCleanDto());
            RegistrarOrdemCriada(ordemServico);
            return CreatedAtAction(nameof(GetById), new { id = ordemServico.Id }, ordemServico);
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { message = ex.Message });
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
            RegistrarTransicao(
                ordemServico,
                StatusOrdemServico.Recebida,
                ordemServico?.CriadaEm,
                ordemServico?.DiagnosticoEm);
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
    public IActionResult EnviarOrcamento(Guid id, [FromBody] HttpDtos.OrdemServicoOrcamentoRequestDto requestDto)
    {
        try
        {
            var ordemServico = _ordensServicoCleanController.EnviarOrcamento(id, requestDto.ParaCleanDto());
            RegistrarTransicao(
                ordemServico,
                StatusOrdemServico.EmDiagnostico,
                ordemServico?.DiagnosticoEm,
                ordemServico?.OrcamentoEnviadoEm);
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
            RegistrarTransicao(
                ordemServico,
                StatusOrdemServico.AguardandoAprovacao,
                ordemServico?.OrcamentoEnviadoEm,
                ordemServico?.ExecucaoIniciadaEm);
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
    public IActionResult RecusarOrcamento(Guid id, [FromBody] HttpDtos.OrdemServicoRespostaAprovacaoRequestDto requestDto)
    {
        try
        {
            var ordemServico = _ordensServicoCleanController.RecusarOrcamento(id, requestDto.ParaCleanDto());
            RegistrarTransicao(
                ordemServico,
                StatusOrdemServico.AguardandoAprovacao,
                ordemServico?.OrcamentoEnviadoEm,
                ordemServico?.DiagnosticoEm);
            return ordemServico is null ? NotFound() : Ok(ordemServico);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/notificacao-orcamento")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OrdemServicoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult NotificarAprovacaoOrcamento(
        Guid id,
        [FromBody] HttpDtos.OrdemServicoNotificacaoOrcamentoRequestDto requestDto)
    {
        try
        {
            var ordemServico = _ordensServicoCleanController.NotificarAprovacaoOrcamento(id, requestDto.ParaCleanDto());
            RegistrarTransicao(
                ordemServico,
                StatusOrdemServico.AguardandoAprovacao,
                ordemServico?.OrcamentoEnviadoEm,
                requestDto.Aprovado == true ? ordemServico?.ExecucaoIniciadaEm : ordemServico?.DiagnosticoEm);
            return ordemServico is null ? NotFound() : Ok(ordemServico);
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { message = ex.Message });
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
            RegistrarCancelamento(ordemServico);
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
            RegistrarTransicao(
                ordemServico,
                StatusOrdemServico.EmExecucao,
                ordemServico?.ExecucaoIniciadaEm,
                ordemServico?.FinalizadaEm);
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
            RegistrarTransicao(
                ordemServico,
                StatusOrdemServico.Finalizada,
                ordemServico?.FinalizadaEm,
                ordemServico?.EntregueEm);
            return ordemServico is null ? NotFound() : Ok(ordemServico);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private static StatusOrdemServico? MapearStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return NormalizarTexto(status) switch
        {
            "recebida" => StatusOrdemServico.Recebida,
            "diagnostico" or "emdiagnostico" => StatusOrdemServico.EmDiagnostico,
            "aguardandoaprovacao" => StatusOrdemServico.AguardandoAprovacao,
            "execucao" or "emexecucao" => StatusOrdemServico.EmExecucao,
            "finalizada" => StatusOrdemServico.Finalizada,
            "entregue" => StatusOrdemServico.Entregue,
            "cancelada" or "cancelado" => StatusOrdemServico.Cancelada,
            _ => throw new ValidacaoException("Status invalido para listagem de ordens de servico.")
        };
    }

    private static OrdemServicoOrdenacaoData MapearOrdenacao(string? ordenacao)
    {
        if (string.IsNullOrWhiteSpace(ordenacao))
        {
            return OrdemServicoOrdenacaoData.MaisAntigasPrimeiro;
        }

        return NormalizarTexto(ordenacao) switch
        {
            "maisantigo" or "maisantigos" or "antigo" or "antigos" or "asc" or "ascendente" =>
                OrdemServicoOrdenacaoData.MaisAntigasPrimeiro,
            "maisnovo" or "maisnovos" or "novo" or "novos" or "desc" or "descendente" =>
                OrdemServicoOrdenacaoData.MaisNovasPrimeiro,
            _ => throw new ValidacaoException("Ordenacao invalida. Use maisAntigo ou maisNovo.")
        };
    }

    private static string NormalizarTexto(string texto)
    {
        var textoSemAcentos = texto.Trim().Normalize(NormalizationForm.FormD);
        var caracteres = textoSemAcentos
            .Where(caractere => CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant);

        return new string(caracteres.ToArray());
    }

    private void RegistrarOrdemCriada(OrdemServicoResponseDto ordemServico)
    {
        _logger.LogInformation(
            "Ordem de servico criada. Evento: {evento}; OrdemServicoId: {ordemServicoId}; ClienteId: {clienteId}; VeiculoId: {veiculoId}; StatusAtual: {statusAtual}",
            "OrdemServicoCriada",
            ordemServico.Id.ToString(),
            ordemServico.ClienteId.ToString(),
            ordemServico.VeiculoId.ToString(),
            ordemServico.Status.ToString());
    }

    private void RegistrarTransicao(
        OrdemServicoResponseDto? ordemServico,
        StatusOrdemServico statusAnterior,
        DateTime? statusAnteriorEm,
        DateTime? statusAtualEm)
    {
        if (ordemServico is null)
        {
            return;
        }

        double? duracaoStatusSegundos = null;

        if (statusAnteriorEm.HasValue && statusAtualEm.HasValue)
        {
            duracaoStatusSegundos = Math.Max(
                0,
                (statusAtualEm.Value - statusAnteriorEm.Value).TotalSeconds);
        }

        _logger.LogInformation(
            "Status da ordem de servico alterado. Evento: {evento}; OrdemServicoId: {ordemServicoId}; StatusAnterior: {statusAnterior}; StatusAtual: {statusAtual}; DuracaoStatusSegundos: {duracaoStatusSegundos}",
            "OrdemServicoStatusAlterado",
            ordemServico.Id.ToString(),
            statusAnterior.ToString(),
            ordemServico.Status.ToString(),
            duracaoStatusSegundos);
    }

    private void RegistrarCancelamento(OrdemServicoResponseDto? ordemServico)
    {
        if (ordemServico is null)
        {
            return;
        }

        _logger.LogInformation(
            "Ordem de servico cancelada. Evento: {evento}; OrdemServicoId: {ordemServicoId}; StatusAtual: {statusAtual}",
            "OrdemServicoCancelada",
            ordemServico.Id.ToString(),
            ordemServico.Status.ToString());
    }
}

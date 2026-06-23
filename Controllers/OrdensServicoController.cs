using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.UseCases.OrdensServico;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdensServicoController : ControllerBase
{
    private readonly ObterTodasOrdensServicoUseCase _obterTodasOrdensServicoUseCase;
    private readonly ObterTempoMedioExecucaoUseCase _obterTempoMedioExecucaoUseCase;
    private readonly ObterOrdensPorCpfCnpjClienteUseCase _obterOrdensPorCpfCnpjClienteUseCase;
    private readonly ObterOrdemServicoPorIdUseCase _obterOrdemServicoPorIdUseCase;
    private readonly CriarOrdemServicoUseCase _criarOrdemServicoUseCase;
    private readonly IniciarDiagnosticoUseCase _iniciarDiagnosticoUseCase;
    private readonly EnviarOrcamentoUseCase _enviarOrcamentoUseCase;
    private readonly AprovarOrcamentoUseCase _aprovarOrcamentoUseCase;
    private readonly RecusarOrcamentoUseCase _recusarOrcamentoUseCase;
    private readonly CancelarOrdemServicoUseCase _cancelarOrdemServicoUseCase;
    private readonly FinalizarOrdemServicoUseCase _finalizarOrdemServicoUseCase;
    private readonly EntregarOrdemServicoUseCase _entregarOrdemServicoUseCase;

    public OrdensServicoController(
        ObterTodasOrdensServicoUseCase obterTodasOrdensServicoUseCase,
        ObterTempoMedioExecucaoUseCase obterTempoMedioExecucaoUseCase,
        ObterOrdensPorCpfCnpjClienteUseCase obterOrdensPorCpfCnpjClienteUseCase,
        ObterOrdemServicoPorIdUseCase obterOrdemServicoPorIdUseCase,
        CriarOrdemServicoUseCase criarOrdemServicoUseCase,
        IniciarDiagnosticoUseCase iniciarDiagnosticoUseCase,
        EnviarOrcamentoUseCase enviarOrcamentoUseCase,
        AprovarOrcamentoUseCase aprovarOrcamentoUseCase,
        RecusarOrcamentoUseCase recusarOrcamentoUseCase,
        CancelarOrdemServicoUseCase cancelarOrdemServicoUseCase,
        FinalizarOrdemServicoUseCase finalizarOrdemServicoUseCase,
        EntregarOrdemServicoUseCase entregarOrdemServicoUseCase)
    {
        _obterTodasOrdensServicoUseCase = obterTodasOrdensServicoUseCase;
        _obterTempoMedioExecucaoUseCase = obterTempoMedioExecucaoUseCase;
        _obterOrdensPorCpfCnpjClienteUseCase = obterOrdensPorCpfCnpjClienteUseCase;
        _obterOrdemServicoPorIdUseCase = obterOrdemServicoPorIdUseCase;
        _criarOrdemServicoUseCase = criarOrdemServicoUseCase;
        _iniciarDiagnosticoUseCase = iniciarDiagnosticoUseCase;
        _enviarOrcamentoUseCase = enviarOrcamentoUseCase;
        _aprovarOrcamentoUseCase = aprovarOrcamentoUseCase;
        _recusarOrcamentoUseCase = recusarOrcamentoUseCase;
        _cancelarOrdemServicoUseCase = cancelarOrdemServicoUseCase;
        _finalizarOrdemServicoUseCase = finalizarOrdemServicoUseCase;
        _entregarOrdemServicoUseCase = entregarOrdemServicoUseCase;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<OrdemServicoResponseDto>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(_obterTodasOrdensServicoUseCase.Executar().Select(MapearResponse).ToList());
    }

    [HttpGet("tempo-medio-execucao")]
    [ProducesResponseType(typeof(TempoMedioExecucaoResponseDto), StatusCodes.Status200OK)]
    public IActionResult GetTempoMedioExecucao()
    {
        var tempoMedio = _obterTempoMedioExecucaoUseCase.Executar();

        return Ok(new TempoMedioExecucaoResponseDto
        {
            QuantidadeOrdensConsideradas = tempoMedio.QuantidadeOrdensConsideradas,
            TempoMedioExecucaoEmMinutos = tempoMedio.TempoMedioExecucaoEmMinutos,
            TempoMedioExecucaoFormatado = tempoMedio.TempoMedioExecucaoFormatado
        });
    }

    [HttpGet("cliente/{cpfCnpj}")]
    [ProducesResponseType(typeof(List<OrdemServicoResponseDto>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public IActionResult GetByCpfCnpjCliente(string cpfCnpj)
    {
        return Ok(_obterOrdensPorCpfCnpjClienteUseCase.Executar(cpfCnpj).Select(MapearResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrdemServicoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var ordemServico = _obterOrdemServicoPorIdUseCase.Executar(id);

        return ordemServico is null ? NotFound() : Ok(MapearResponse(ordemServico));
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrdemServicoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Post([FromBody] OrdemServicoRequestDto ordemServicoRequestDto)
    {
        try
        {
            var input = new CriarOrdemServicoInput(ordemServicoRequestDto.CpfCnpj, ordemServicoRequestDto.VeiculoId, ordemServicoRequestDto.Veiculo, ordemServicoRequestDto.Cliente);
            var ordemServico = MapearResponse(_criarOrdemServicoUseCase.Executar(input));
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
            var ordemServico = _iniciarDiagnosticoUseCase.Executar(id);
            return ordemServico is null ? NotFound() : Ok(MapearResponse(ordemServico));
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
            var input = new OrdemServicoOrcamentoInput(
                requestDto.ServicoIds,
                requestDto.PecasInsumos
                    .Select(item => new OrdemServicoItemPecaInsumoInput(item.PecaInsumoId, item.Quantidade))
                    .ToList());
            var ordemServico = _enviarOrcamentoUseCase.Executar(id, input);
            return ordemServico is null ? NotFound() : Ok(MapearResponse(ordemServico));
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
            var ordemServico = _aprovarOrcamentoUseCase.Executar(id);
            return ordemServico is null ? NotFound() : Ok(MapearResponse(ordemServico));
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
            var ordemServico = _recusarOrcamentoUseCase.Executar(id, new RecusarOrcamentoInput(requestDto.MotivoRecusa));
            return ordemServico is null ? NotFound() : Ok(MapearResponse(ordemServico));
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
            var ordemServico = _cancelarOrdemServicoUseCase.Executar(id);
            return ordemServico is null ? NotFound() : Ok(MapearResponse(ordemServico));
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
            var ordemServico = _finalizarOrdemServicoUseCase.Executar(id);
            return ordemServico is null ? NotFound() : Ok(MapearResponse(ordemServico));
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
            var ordemServico = _entregarOrdemServicoUseCase.Executar(id);
            return ordemServico is null ? NotFound() : Ok(MapearResponse(ordemServico));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private static OrdemServicoResponseDto MapearResponse(OrdemServicoOutput ordemServico)
    {
        return new OrdemServicoResponseDto
        {
            Id = ordemServico.Id,
            ClienteId = ordemServico.ClienteId,
            ClienteNome = ordemServico.ClienteNome,
            ClienteCpfCnpj = ordemServico.ClienteCpfCnpj,
            VeiculoId = ordemServico.VeiculoId,
            PlacaVeiculo = ordemServico.PlacaVeiculo,
            ModeloVeiculo = ordemServico.ModeloVeiculo,
            Status = ordemServico.Status,
            StatusAprovacaoOrcamento = ordemServico.StatusAprovacaoOrcamento,
            ValorTotalServicos = ordemServico.ValorTotalServicos,
            ValorTotalPecasInsumos = ordemServico.ValorTotalPecasInsumos,
            ValorTotalOrcamento = ordemServico.ValorTotalOrcamento,
            EnvioOrcamento = ordemServico.EnvioOrcamento,
            MotivoRecusaOrcamento = ordemServico.MotivoRecusaOrcamento,
            CriadaEm = ordemServico.CriadaEm,
            DiagnosticoEm = ordemServico.DiagnosticoEm,
            OrcamentoEnviadoEm = ordemServico.OrcamentoEnviadoEm,
            ExecucaoIniciadaEm = ordemServico.ExecucaoIniciadaEm,
            FinalizadaEm = ordemServico.FinalizadaEm,
            EntregueEm = ordemServico.EntregueEm,
            ItensServico = ordemServico.ItensServico.Select(item => new OrdemServicoItemServicoResponseDto
            {
                ServicoId = item.ServicoId,
                NomeServico = item.NomeServico,
                PrecoServico = item.PrecoServico
            }).ToList(),
            ItensPecaInsumo = ordemServico.ItensPecaInsumo.Select(item => new OrdemServicoItemPecaInsumoResponseDto
            {
                PecaInsumoId = item.PecaInsumoId,
                NomePecaInsumo = item.NomePecaInsumo,
                PrecoUnitario = item.PrecoUnitario,
                Quantidade = item.Quantidade,
                Subtotal = item.Subtotal
            }).ToList()
        };
    }
}

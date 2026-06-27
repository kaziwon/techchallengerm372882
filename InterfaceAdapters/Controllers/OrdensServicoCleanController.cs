using OficinaMecanica.Api.InterfaceAdapters.DTOs;
using OficinaMecanica.Api.Application.UseCases.OrdensServico;

namespace OficinaMecanica.Api.InterfaceAdapters.Controllers;

public class OrdensServicoCleanController
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

    public OrdensServicoCleanController(
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

    public List<OrdemServicoResponseDto> ObterTodas()
    {
        return _obterTodasOrdensServicoUseCase.Executar().Select(MapearResponse).ToList();
    }

    public TempoMedioExecucaoResponseDto ObterTempoMedioExecucao()
    {
        var tempoMedio = _obterTempoMedioExecucaoUseCase.Executar();

        return new TempoMedioExecucaoResponseDto
        {
            QuantidadeOrdensConsideradas = tempoMedio.QuantidadeOrdensConsideradas,
            TempoMedioExecucaoEmMinutos = tempoMedio.TempoMedioExecucaoEmMinutos,
            TempoMedioExecucaoFormatado = tempoMedio.TempoMedioExecucaoFormatado
        };
    }

    public List<OrdemServicoResponseDto> ObterPorCpfCnpjCliente(string cpfCnpj)
    {
        return _obterOrdensPorCpfCnpjClienteUseCase.Executar(cpfCnpj).Select(MapearResponse).ToList();
    }

    public OrdemServicoResponseDto? ObterPorId(Guid id)
    {
        var ordemServico = _obterOrdemServicoPorIdUseCase.Executar(id);

        return ordemServico is null ? null : MapearResponse(ordemServico);
    }

    public OrdemServicoResponseDto Criar(OrdemServicoRequestDto ordemServicoRequestDto)
    {
        var input = new CriarOrdemServicoInput(
            ordemServicoRequestDto.CpfCnpj,
            ordemServicoRequestDto.VeiculoId,
            MapearClienteInput(ordemServicoRequestDto.Cliente),
            MapearVeiculoInput(ordemServicoRequestDto.Veiculo),
            ordemServicoRequestDto.ServicoIds,
            ordemServicoRequestDto.PecasInsumos
                .Select(item => new OrdemServicoItemPecaInsumoInput(item.PecaInsumoId, item.Quantidade))
                .ToList());

        return MapearResponse(_criarOrdemServicoUseCase.Executar(input));
    }

    public OrdemServicoResponseDto? IniciarDiagnostico(Guid id)
    {
        var ordemServico = _iniciarDiagnosticoUseCase.Executar(id);

        return ordemServico is null ? null : MapearResponse(ordemServico);
    }

    public OrdemServicoResponseDto? EnviarOrcamento(Guid id, OrdemServicoOrcamentoRequestDto requestDto)
    {
        var input = new OrdemServicoOrcamentoInput(
            requestDto.ServicoIds,
            requestDto.PecasInsumos
                .Select(item => new OrdemServicoItemPecaInsumoInput(item.PecaInsumoId, item.Quantidade))
                .ToList());
        var ordemServico = _enviarOrcamentoUseCase.Executar(id, input);

        return ordemServico is null ? null : MapearResponse(ordemServico);
    }

    public OrdemServicoResponseDto? AprovarOrcamento(Guid id)
    {
        var ordemServico = _aprovarOrcamentoUseCase.Executar(id);

        return ordemServico is null ? null : MapearResponse(ordemServico);
    }

    public OrdemServicoResponseDto? RecusarOrcamento(Guid id, OrdemServicoRespostaAprovacaoRequestDto requestDto)
    {
        var ordemServico = _recusarOrcamentoUseCase.Executar(id, new RecusarOrcamentoInput(requestDto.MotivoRecusa));

        return ordemServico is null ? null : MapearResponse(ordemServico);
    }

    public OrdemServicoResponseDto? NotificarAprovacaoOrcamento(
        Guid id,
        OrdemServicoNotificacaoOrcamentoRequestDto requestDto)
    {
        if (requestDto.Aprovado)
        {
            return AprovarOrcamento(id);
        }

        return RecusarOrcamento(id, new OrdemServicoRespostaAprovacaoRequestDto
        {
            MotivoRecusa = requestDto.MotivoRecusa
        });
    }

    public OrdemServicoResponseDto? Cancelar(Guid id)
    {
        var ordemServico = _cancelarOrdemServicoUseCase.Executar(id);

        return ordemServico is null ? null : MapearResponse(ordemServico);
    }

    public OrdemServicoResponseDto? Finalizar(Guid id)
    {
        var ordemServico = _finalizarOrdemServicoUseCase.Executar(id);

        return ordemServico is null ? null : MapearResponse(ordemServico);
    }

    public OrdemServicoResponseDto? Entregar(Guid id)
    {
        var ordemServico = _entregarOrdemServicoUseCase.Executar(id);

        return ordemServico is null ? null : MapearResponse(ordemServico);
    }

    private static CriarOrdemServicoClienteInput? MapearClienteInput(OrdemServicoClienteRequestDto? cliente)
    {
        return cliente is null
            ? null
            : new CriarOrdemServicoClienteInput(
                cliente.Nome,
                cliente.CpfCnpj,
                cliente.Email,
                cliente.Telefone);
    }

    private static CriarOrdemServicoVeiculoInput? MapearVeiculoInput(OrdemServicoVeiculoRequestDto? veiculo)
    {
        return veiculo is null
            ? null
            : new CriarOrdemServicoVeiculoInput(
                veiculo.Placa,
                veiculo.Marca,
                veiculo.Modelo,
                veiculo.Ano);
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

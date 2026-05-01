using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.Interfaces;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.Domain.Repositories;

namespace OficinaMecanica.Api.Application.Services;

public class OrdemServicoService : IOrdemServicoService
{
    private const string ClienteNaoEncontrado = "Cliente nao encontrado para o CPF/CNPJ informado.";
    private const string VeiculoNaoEncontrado = "Veiculo nao encontrado.";
    private const string VeiculoNaoPertenceAoCliente = "O veiculo informado nao pertence ao cliente.";
    private const string ServicoNaoEncontrado = "Um ou mais servicos informados nao foram encontrados.";
    private const string PecaInsumoNaoEncontrado = "Uma ou mais pecas/insumos informados nao foram encontrados.";
    private const string OrdemServicoNaoEstaRecebida = "A ordem de servico nao esta recebida.";
    private const string OrdemServicoNaoEstaEmDiagnostico = "A ordem de servico nao esta em diagnostico.";
    private const string OrcamentoNaoEstaAguardandoAprovacao = "A ordem de servico nao esta aguardando aprovacao do orcamento.";
    private const string OrdemServicoNaoEstaEmExecucao = "A ordem de servico nao esta em execucao.";
    private const string OrdemServicoNaoEstaFinalizada = "A ordem de servico nao esta finalizada.";
    private const string OrdemServicoNaoPodeSerCancelada = "A ordem de servico nao pode ser cancelada no status atual.";
    private const string EstoqueInsuficiente = "Nao ha estoque suficiente para uma ou mais pecas/insumos.";

    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IPecaInsumoRepository _pecaInsumoRepository;

    public OrdemServicoService(
        IOrdemServicoRepository ordemServicoRepository,
        IClienteRepository clienteRepository,
        IVeiculoRepository veiculoRepository,
        IServicoRepository servicoRepository,
        IPecaInsumoRepository pecaInsumoRepository)
    {
        _ordemServicoRepository = ordemServicoRepository;
        _clienteRepository = clienteRepository;
        _veiculoRepository = veiculoRepository;
        _servicoRepository = servicoRepository;
        _pecaInsumoRepository = pecaInsumoRepository;
    }

    public List<OrdemServicoResponseDto> ObterTodas()
    {
        return _ordemServicoRepository.ObterTodas().Select(MapearParaResponse).ToList();
    }

    public TempoMedioExecucaoResponseDto ObterTempoMedioExecucao()
    {
        var ordensFinalizadas = _ordemServicoRepository.ObterTodas()
            .Where(ordemServico => ordemServico.ExecucaoIniciadaEm.HasValue && ordemServico.FinalizadaEm.HasValue)
            .ToList();

        if (ordensFinalizadas.Count == 0)
        {
            return new TempoMedioExecucaoResponseDto
            {
                QuantidadeOrdensConsideradas = 0,
                TempoMedioExecucaoEmMinutos = 0,
                TempoMedioExecucaoFormatado = "00:00:00"
            };
        }

        var media = TimeSpan.FromTicks((long)ordensFinalizadas
            .Average(ordemServico => (ordemServico.FinalizadaEm!.Value - ordemServico.ExecucaoIniciadaEm!.Value).Ticks));

        return new TempoMedioExecucaoResponseDto
        {
            QuantidadeOrdensConsideradas = ordensFinalizadas.Count,
            TempoMedioExecucaoEmMinutos = Math.Round(media.TotalMinutes, 2),
            TempoMedioExecucaoFormatado = media.ToString(@"hh\:mm\:ss")
        };
    }

    public List<OrdemServicoResponseDto> ObterPorCpfCnpjCliente(string cpfCnpj)
    {
        var cpfCnpjNormalizado = NormalizarCpfCnpj(cpfCnpj);

        return _ordemServicoRepository
            .ObterPorCpfCnpjCliente(cpfCnpjNormalizado)
            .Select(MapearParaResponse)
            .ToList();
    }

    public OrdemServicoResponseDto? ObterPorId(Guid id)
    {
        var ordemServico = _ordemServicoRepository.ObterPorId(id);

        return ordemServico is null ? null : MapearParaResponse(ordemServico);
    }

    public OrdemServicoResponseDto Criar(OrdemServicoRequestDto ordemServicoRequestDto)
    {
        var cliente = _clienteRepository.ObterPorCpfCnpj(NormalizarCpfCnpj(ordemServicoRequestDto.CpfCnpj));

        if (cliente is null)
        {
            throw new InvalidOperationException(ClienteNaoEncontrado);
        }

        var veiculo = _veiculoRepository.ObterPorId(ordemServicoRequestDto.VeiculoId);

        if (veiculo is null)
        {
            throw new InvalidOperationException(VeiculoNaoEncontrado);
        }

        if (veiculo.ClienteId != cliente.Id)
        {
            throw new InvalidOperationException(VeiculoNaoPertenceAoCliente);
        }

        var agora = DateTime.UtcNow;

        var ordemServico = new OrdemServico
        {
            Id = Guid.NewGuid(),
            ClienteId = cliente.Id,
            VeiculoId = veiculo.Id,
            Status = StatusOrdemServico.Recebida,
            StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Pendente,
            ValorTotalServicos = 0,
            ValorTotalPecasInsumos = 0,
            ValorTotalOrcamento = 0,
            EnvioOrcamento = string.Empty,
            CriadaEm = agora
        };

        return MapearParaResponse(_ordemServicoRepository.Adicionar(ordemServico));
    }

    public OrdemServicoResponseDto? IniciarDiagnostico(Guid id)
    {
        var ordemServico = _ordemServicoRepository.ObterPorId(id);

        if (ordemServico is null)
        {
            return null;
        }

        if (ordemServico.Status != StatusOrdemServico.Recebida)
        {
            throw new InvalidOperationException(OrdemServicoNaoEstaRecebida);
        }

        ordemServico.Status = StatusOrdemServico.EmDiagnostico;
        ordemServico.DiagnosticoEm = DateTime.UtcNow;

        return MapearParaResponse(_ordemServicoRepository.Atualizar(ordemServico)!);
    }

    public OrdemServicoResponseDto? EnviarOrcamento(Guid id, OrdemServicoOrcamentoRequestDto requestDto)
    {
        var ordemServico = _ordemServicoRepository.ObterPorId(id);

        if (ordemServico is null)
        {
            return null;
        }

        if (ordemServico.Status != StatusOrdemServico.EmDiagnostico)
        {
            throw new InvalidOperationException(OrdemServicoNaoEstaEmDiagnostico);
        }

        var itensServico = new List<OrdemServicoItemServico>();
        decimal valorTotalServicos = 0;

        foreach (var servicoId in requestDto.ServicoIds)
        {
            var servico = _servicoRepository.ObterPorId(servicoId);

            if (servico is null)
            {
                throw new InvalidOperationException(ServicoNaoEncontrado);
            }

            itensServico.Add(new OrdemServicoItemServico
            {
                Id = Guid.NewGuid(),
                OrdemServicoId = ordemServico.Id,
                ServicoId = servico.Id,
                NomeServico = servico.Nome,
                PrecoServico = servico.Preco
            });

            valorTotalServicos += servico.Preco;
        }

        var itensPecaInsumo = new List<OrdemServicoItemPecaInsumo>();
        decimal valorTotalPecasInsumos = 0;

        foreach (var itemPeca in requestDto.PecasInsumos)
        {
            var pecaInsumo = _pecaInsumoRepository.ObterPorId(itemPeca.PecaInsumoId);

            if (pecaInsumo is null)
            {
                throw new InvalidOperationException(PecaInsumoNaoEncontrado);
            }

            var subtotal = pecaInsumo.PrecoUnitario * itemPeca.Quantidade;

            itensPecaInsumo.Add(new OrdemServicoItemPecaInsumo
            {
                Id = Guid.NewGuid(),
                OrdemServicoId = ordemServico.Id,
                PecaInsumoId = pecaInsumo.Id,
                NomePecaInsumo = pecaInsumo.Nome,
                PrecoUnitario = pecaInsumo.PrecoUnitario,
                Quantidade = itemPeca.Quantidade,
                Subtotal = subtotal
            });

            valorTotalPecasInsumos += subtotal;
        }

        var agora = DateTime.UtcNow;
        var valorTotalOrcamento = valorTotalServicos + valorTotalPecasInsumos;

        ordemServico.ItensServico.Clear();
        foreach (var itemServico in itensServico)
        {
            ordemServico.ItensServico.Add(itemServico);
        }

        ordemServico.ItensPecaInsumo.Clear();
        foreach (var itemPecaInsumo in itensPecaInsumo)
        {
            ordemServico.ItensPecaInsumo.Add(itemPecaInsumo);
        }

        ordemServico.ValorTotalServicos = valorTotalServicos;
        ordemServico.ValorTotalPecasInsumos = valorTotalPecasInsumos;
        ordemServico.ValorTotalOrcamento = valorTotalOrcamento;
        ordemServico.Status = StatusOrdemServico.AguardandoAprovacao;
        ordemServico.StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Pendente;
        ordemServico.OrcamentoEnviadoEm = agora;
        ordemServico.EnvioOrcamento = $"Orcamento enviado para {ordemServico.Cliente?.Email}";

        return MapearParaResponse(_ordemServicoRepository.Atualizar(ordemServico)!);
    }

    public OrdemServicoResponseDto? AprovarOrcamento(Guid id)
    {
        var ordemServico = _ordemServicoRepository.ObterPorId(id);

        if (ordemServico is null)
        {
            return null;
        }

        if (ordemServico.Status != StatusOrdemServico.AguardandoAprovacao)
        {
            throw new InvalidOperationException(OrcamentoNaoEstaAguardandoAprovacao);
        }

        foreach (var itemPecaInsumo in ordemServico.ItensPecaInsumo)
        {
            var pecaInsumo = _pecaInsumoRepository.ObterPorId(itemPecaInsumo.PecaInsumoId);

            if (pecaInsumo is null)
            {
                throw new InvalidOperationException(PecaInsumoNaoEncontrado);
            }

            if (pecaInsumo.QuantidadeEstoque < itemPecaInsumo.Quantidade)
            {
                throw new InvalidOperationException(EstoqueInsuficiente);
            }

            pecaInsumo.QuantidadeEstoque -= itemPecaInsumo.Quantidade;
            _pecaInsumoRepository.Atualizar(pecaInsumo);
        }

        ordemServico.Status = StatusOrdemServico.EmExecucao;
        ordemServico.StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Aprovado;
        ordemServico.ExecucaoIniciadaEm = DateTime.UtcNow;
        ordemServico.MotivoRecusaOrcamento = string.Empty;

        return MapearParaResponse(_ordemServicoRepository.Atualizar(ordemServico)!);
    }

    public OrdemServicoResponseDto? RecusarOrcamento(Guid id, OrdemServicoRespostaAprovacaoRequestDto requestDto)
    {
        var ordemServico = _ordemServicoRepository.ObterPorId(id);

        if (ordemServico is null)
        {
            return null;
        }

        if (ordemServico.Status != StatusOrdemServico.AguardandoAprovacao)
        {
            throw new InvalidOperationException(OrcamentoNaoEstaAguardandoAprovacao);
        }

        ordemServico.Status = StatusOrdemServico.EmDiagnostico;
        ordemServico.StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Recusado;
        ordemServico.MotivoRecusaOrcamento = requestDto.MotivoRecusa;

        return MapearParaResponse(_ordemServicoRepository.Atualizar(ordemServico)!);
    }

    public OrdemServicoResponseDto? Cancelar(Guid id)
    {
        var ordemServico = _ordemServicoRepository.ObterPorId(id);

        if (ordemServico is null)
        {
            return null;
        }

        if (ordemServico.Status is StatusOrdemServico.Entregue or StatusOrdemServico.Finalizada or StatusOrdemServico.Cancelada)
        {
            throw new InvalidOperationException(OrdemServicoNaoPodeSerCancelada);
        }

        if (ordemServico.Status == StatusOrdemServico.EmExecucao)
        {
            foreach (var itemPecaInsumo in ordemServico.ItensPecaInsumo)
            {
                var pecaInsumo = _pecaInsumoRepository.ObterPorId(itemPecaInsumo.PecaInsumoId);

                if (pecaInsumo is null)
                {
                    throw new InvalidOperationException(PecaInsumoNaoEncontrado);
                }

                pecaInsumo.QuantidadeEstoque += itemPecaInsumo.Quantidade;
                _pecaInsumoRepository.Atualizar(pecaInsumo);
            }
        }

        ordemServico.Status = StatusOrdemServico.Cancelada;

        return MapearParaResponse(_ordemServicoRepository.Atualizar(ordemServico)!);
    }

    public OrdemServicoResponseDto? Finalizar(Guid id)
    {
        var ordemServico = _ordemServicoRepository.ObterPorId(id);

        if (ordemServico is null)
        {
            return null;
        }

        if (ordemServico.Status != StatusOrdemServico.EmExecucao)
        {
            throw new InvalidOperationException(OrdemServicoNaoEstaEmExecucao);
        }

        ordemServico.Status = StatusOrdemServico.Finalizada;
        ordemServico.FinalizadaEm = DateTime.UtcNow;

        return MapearParaResponse(_ordemServicoRepository.Atualizar(ordemServico)!);
    }

    public OrdemServicoResponseDto? Entregar(Guid id)
    {
        var ordemServico = _ordemServicoRepository.ObterPorId(id);

        if (ordemServico is null)
        {
            return null;
        }

        if (ordemServico.Status != StatusOrdemServico.Finalizada)
        {
            throw new InvalidOperationException(OrdemServicoNaoEstaFinalizada);
        }

        ordemServico.Status = StatusOrdemServico.Entregue;
        ordemServico.EntregueEm = DateTime.UtcNow;

        return MapearParaResponse(_ordemServicoRepository.Atualizar(ordemServico)!);
    }

    private static string NormalizarCpfCnpj(string cpfCnpj)
    {
        return new string(cpfCnpj.Where(char.IsDigit).ToArray());
    }

    private static OrdemServicoResponseDto MapearParaResponse(OrdemServico ordemServico)
    {
        return new OrdemServicoResponseDto
        {
            Id = ordemServico.Id,
            ClienteId = ordemServico.ClienteId,
            ClienteNome = ordemServico.Cliente?.Nome ?? string.Empty,
            ClienteCpfCnpj = ordemServico.Cliente?.CpfCnpj ?? string.Empty,
            VeiculoId = ordemServico.VeiculoId,
            PlacaVeiculo = ordemServico.Veiculo?.Placa ?? string.Empty,
            ModeloVeiculo = ordemServico.Veiculo?.Modelo ?? string.Empty,
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

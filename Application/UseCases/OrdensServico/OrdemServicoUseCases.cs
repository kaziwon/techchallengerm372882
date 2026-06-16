using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public record CriarOrdemServicoInput(string CpfCnpj, Guid VeiculoId);
public record OrdemServicoItemPecaInsumoInput(Guid PecaInsumoId, int Quantidade);
public record OrdemServicoOrcamentoInput(List<Guid> ServicoIds, List<OrdemServicoItemPecaInsumoInput> PecasInsumos);
public record RecusarOrcamentoInput(string MotivoRecusa);

public class TempoMedioExecucaoOutput
{
    public int QuantidadeOrdensConsideradas { get; set; }
    public double TempoMedioExecucaoEmMinutos { get; set; }
    public string TempoMedioExecucaoFormatado { get; set; } = string.Empty;
}

public class OrdemServicoOutput
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteCpfCnpj { get; set; } = string.Empty;
    public Guid VeiculoId { get; set; }
    public string PlacaVeiculo { get; set; } = string.Empty;
    public string ModeloVeiculo { get; set; } = string.Empty;
    public StatusOrdemServico Status { get; set; }
    public StatusAprovacaoOrcamento StatusAprovacaoOrcamento { get; set; }
    public decimal ValorTotalServicos { get; set; }
    public decimal ValorTotalPecasInsumos { get; set; }
    public decimal ValorTotalOrcamento { get; set; }
    public string EnvioOrcamento { get; set; } = string.Empty;
    public string MotivoRecusaOrcamento { get; set; } = string.Empty;
    public DateTime CriadaEm { get; set; }
    public DateTime? DiagnosticoEm { get; set; }
    public DateTime? OrcamentoEnviadoEm { get; set; }
    public DateTime? ExecucaoIniciadaEm { get; set; }
    public DateTime? FinalizadaEm { get; set; }
    public DateTime? EntregueEm { get; set; }
    public List<OrdemServicoItemServicoOutput> ItensServico { get; set; } = [];
    public List<OrdemServicoItemPecaInsumoOutput> ItensPecaInsumo { get; set; } = [];
}

public class OrdemServicoItemServicoOutput
{
    public Guid ServicoId { get; set; }
    public string NomeServico { get; set; } = string.Empty;
    public decimal PrecoServico { get; set; }
}

public class OrdemServicoItemPecaInsumoOutput
{
    public Guid PecaInsumoId { get; set; }
    public string NomePecaInsumo { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Quantidade { get; set; }
    public decimal Subtotal { get; set; }
}

public class ObterTodasOrdensServicoUseCase
{
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public ObterTodasOrdensServicoUseCase(IOrdemServicoGateway ordemServicoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
    }

    public List<OrdemServicoOutput> Executar()
    {
        return _ordemServicoGateway.ObterTodas().Select(OrdemServicoOutputMapper.Mapear).ToList();
    }
}

public class ObterTempoMedioExecucaoUseCase
{
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public ObterTempoMedioExecucaoUseCase(IOrdemServicoGateway ordemServicoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
    }

    public TempoMedioExecucaoOutput Executar()
    {
        var ordensFinalizadas = _ordemServicoGateway.ObterTodas()
            .Where(ordemServico => ordemServico.ExecucaoIniciadaEm.HasValue && ordemServico.FinalizadaEm.HasValue)
            .ToList();

        if (ordensFinalizadas.Count == 0)
        {
            return new TempoMedioExecucaoOutput
            {
                QuantidadeOrdensConsideradas = 0,
                TempoMedioExecucaoEmMinutos = 0,
                TempoMedioExecucaoFormatado = "00:00:00"
            };
        }

        var media = TimeSpan.FromTicks((long)ordensFinalizadas
            .Average(ordemServico => (ordemServico.FinalizadaEm!.Value - ordemServico.ExecucaoIniciadaEm!.Value).Ticks));

        return new TempoMedioExecucaoOutput
        {
            QuantidadeOrdensConsideradas = ordensFinalizadas.Count,
            TempoMedioExecucaoEmMinutos = Math.Round(media.TotalMinutes, 2),
            TempoMedioExecucaoFormatado = media.ToString(@"hh\:mm\:ss")
        };
    }
}

public class ObterOrdensPorCpfCnpjClienteUseCase
{
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public ObterOrdensPorCpfCnpjClienteUseCase(IOrdemServicoGateway ordemServicoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
    }

    public List<OrdemServicoOutput> Executar(string cpfCnpj)
    {
        return _ordemServicoGateway
            .ObterPorCpfCnpjCliente(NormalizarCpfCnpj(cpfCnpj))
            .Select(OrdemServicoOutputMapper.Mapear)
            .ToList();
    }

    private static string NormalizarCpfCnpj(string cpfCnpj)
    {
        return new string(cpfCnpj.Where(char.IsDigit).ToArray());
    }
}

public class ObterOrdemServicoPorIdUseCase
{
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public ObterOrdemServicoPorIdUseCase(IOrdemServicoGateway ordemServicoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
    }

    public OrdemServicoOutput? Executar(Guid id)
    {
        var ordemServico = _ordemServicoGateway.ObterPorId(id);

        return ordemServico is null ? null : OrdemServicoOutputMapper.Mapear(ordemServico);
    }
}

public class CriarOrdemServicoUseCase
{
    private const string ClienteNaoEncontrado = "Cliente nao encontrado para o CPF/CNPJ informado.";
    private const string VeiculoNaoEncontrado = "Veiculo nao encontrado.";
    private const string VeiculoNaoPertenceAoCliente = "O veiculo informado nao pertence ao cliente.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;
    private readonly IClienteGateway _clienteGateway;
    private readonly IVeiculoGateway _veiculoGateway;

    public CriarOrdemServicoUseCase(
        IOrdemServicoGateway ordemServicoGateway,
        IClienteGateway clienteGateway,
        IVeiculoGateway veiculoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
        _clienteGateway = clienteGateway;
        _veiculoGateway = veiculoGateway;
    }

    public OrdemServicoOutput Executar(CriarOrdemServicoInput input)
    {
        var cliente = _clienteGateway.ObterPorCpfCnpj(NormalizarCpfCnpj(input.CpfCnpj));

        if (cliente is null)
        {
            throw new InvalidOperationException(ClienteNaoEncontrado);
        }

        var veiculo = _veiculoGateway.ObterPorId(input.VeiculoId);

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

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Adicionar(ordemServico));
    }

    private static string NormalizarCpfCnpj(string cpfCnpj)
    {
        return new string(cpfCnpj.Where(char.IsDigit).ToArray());
    }
}

public class IniciarDiagnosticoUseCase
{
    private const string OrdemServicoNaoEstaRecebida = "A ordem de servico nao esta recebida.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public IniciarDiagnosticoUseCase(IOrdemServicoGateway ordemServicoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
    }

    public OrdemServicoOutput? Executar(Guid id)
    {
        var ordemServico = _ordemServicoGateway.ObterPorId(id);

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

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Atualizar(ordemServico)!);
    }
}

public class EnviarOrcamentoUseCase
{
    private const string ServicoNaoEncontrado = "Um ou mais servicos informados nao foram encontrados.";
    private const string PecaInsumoNaoEncontrado = "Uma ou mais pecas/insumos informados nao foram encontrados.";
    private const string OrdemServicoNaoEstaEmDiagnostico = "A ordem de servico nao esta em diagnostico.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;
    private readonly IServicoGateway _servicoGateway;
    private readonly IPecaInsumoGateway _pecaInsumoGateway;

    public EnviarOrcamentoUseCase(
        IOrdemServicoGateway ordemServicoGateway,
        IServicoGateway servicoGateway,
        IPecaInsumoGateway pecaInsumoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
        _servicoGateway = servicoGateway;
        _pecaInsumoGateway = pecaInsumoGateway;
    }

    public OrdemServicoOutput? Executar(Guid id, OrdemServicoOrcamentoInput input)
    {
        var ordemServico = _ordemServicoGateway.ObterPorId(id);

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

        foreach (var servicoId in input.ServicoIds)
        {
            var servico = _servicoGateway.ObterPorId(servicoId);

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

        foreach (var itemPeca in input.PecasInsumos)
        {
            var pecaInsumo = _pecaInsumoGateway.ObterPorId(itemPeca.PecaInsumoId);

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

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Atualizar(ordemServico)!);
    }
}

public class AprovarOrcamentoUseCase
{
    private const string PecaInsumoNaoEncontrado = "Uma ou mais pecas/insumos informados nao foram encontrados.";
    private const string OrcamentoNaoEstaAguardandoAprovacao = "A ordem de servico nao esta aguardando aprovacao do orcamento.";
    private const string EstoqueInsuficiente = "Nao ha estoque suficiente para uma ou mais pecas/insumos.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;
    private readonly IPecaInsumoGateway _pecaInsumoGateway;

    public AprovarOrcamentoUseCase(IOrdemServicoGateway ordemServicoGateway, IPecaInsumoGateway pecaInsumoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
        _pecaInsumoGateway = pecaInsumoGateway;
    }

    public OrdemServicoOutput? Executar(Guid id)
    {
        var ordemServico = _ordemServicoGateway.ObterPorId(id);

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
            var pecaInsumo = _pecaInsumoGateway.ObterPorId(itemPecaInsumo.PecaInsumoId);

            if (pecaInsumo is null)
            {
                throw new InvalidOperationException(PecaInsumoNaoEncontrado);
            }

            if (pecaInsumo.QuantidadeEstoque < itemPecaInsumo.Quantidade)
            {
                throw new InvalidOperationException(EstoqueInsuficiente);
            }

            pecaInsumo.QuantidadeEstoque -= itemPecaInsumo.Quantidade;
            _pecaInsumoGateway.Atualizar(pecaInsumo);
        }

        ordemServico.Status = StatusOrdemServico.EmExecucao;
        ordemServico.StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Aprovado;
        ordemServico.ExecucaoIniciadaEm = DateTime.UtcNow;
        ordemServico.MotivoRecusaOrcamento = string.Empty;

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Atualizar(ordemServico)!);
    }
}

public class RecusarOrcamentoUseCase
{
    private const string OrcamentoNaoEstaAguardandoAprovacao = "A ordem de servico nao esta aguardando aprovacao do orcamento.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public RecusarOrcamentoUseCase(IOrdemServicoGateway ordemServicoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
    }

    public OrdemServicoOutput? Executar(Guid id, RecusarOrcamentoInput input)
    {
        var ordemServico = _ordemServicoGateway.ObterPorId(id);

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
        ordemServico.MotivoRecusaOrcamento = input.MotivoRecusa;

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Atualizar(ordemServico)!);
    }
}

public class CancelarOrdemServicoUseCase
{
    private const string PecaInsumoNaoEncontrado = "Uma ou mais pecas/insumos informados nao foram encontrados.";
    private const string OrdemServicoNaoPodeSerCancelada = "A ordem de servico nao pode ser cancelada no status atual.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;
    private readonly IPecaInsumoGateway _pecaInsumoGateway;

    public CancelarOrdemServicoUseCase(IOrdemServicoGateway ordemServicoGateway, IPecaInsumoGateway pecaInsumoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
        _pecaInsumoGateway = pecaInsumoGateway;
    }

    public OrdemServicoOutput? Executar(Guid id)
    {
        var ordemServico = _ordemServicoGateway.ObterPorId(id);

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
                var pecaInsumo = _pecaInsumoGateway.ObterPorId(itemPecaInsumo.PecaInsumoId);

                if (pecaInsumo is null)
                {
                    throw new InvalidOperationException(PecaInsumoNaoEncontrado);
                }

                pecaInsumo.QuantidadeEstoque += itemPecaInsumo.Quantidade;
                _pecaInsumoGateway.Atualizar(pecaInsumo);
            }
        }

        ordemServico.Status = StatusOrdemServico.Cancelada;

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Atualizar(ordemServico)!);
    }
}

public class FinalizarOrdemServicoUseCase
{
    private const string OrdemServicoNaoEstaEmExecucao = "A ordem de servico nao esta em execucao.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public FinalizarOrdemServicoUseCase(IOrdemServicoGateway ordemServicoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
    }

    public OrdemServicoOutput? Executar(Guid id)
    {
        var ordemServico = _ordemServicoGateway.ObterPorId(id);

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

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Atualizar(ordemServico)!);
    }
}

public class EntregarOrdemServicoUseCase
{
    private const string OrdemServicoNaoEstaFinalizada = "A ordem de servico nao esta finalizada.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public EntregarOrdemServicoUseCase(IOrdemServicoGateway ordemServicoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
    }

    public OrdemServicoOutput? Executar(Guid id)
    {
        var ordemServico = _ordemServicoGateway.ObterPorId(id);

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

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Atualizar(ordemServico)!);
    }
}

internal static class OrdemServicoOutputMapper
{
    public static OrdemServicoOutput Mapear(OrdemServico ordemServico)
    {
        return new OrdemServicoOutput
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
            ItensServico = ordemServico.ItensServico.Select(item => new OrdemServicoItemServicoOutput
            {
                ServicoId = item.ServicoId,
                NomeServico = item.NomeServico,
                PrecoServico = item.PrecoServico
            }).ToList(),
            ItensPecaInsumo = ordemServico.ItensPecaInsumo.Select(item => new OrdemServicoItemPecaInsumoOutput
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

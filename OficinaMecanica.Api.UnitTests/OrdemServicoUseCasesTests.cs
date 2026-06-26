using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Application.Exceptions;
using OficinaMecanica.Api.Application.UseCases.OrdensServico;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.UnitTests;

public class OrdemServicoUseCasesTests
{
    [Fact]
    public void Criar_DeveLancarExcecao_QuandoClienteNaoExistir()
    {
        var contexto = new FakeContextoOrdemServico();
        var useCase = contexto.CriarOrdemServicoUseCase();

        Assert.Throws<InvalidOperationException>(() => useCase.Executar(new CriarOrdemServicoInput("39053344705", Guid.NewGuid())));
    }

    [Fact]
    public void Criar_DeveLancarExcecao_QuandoVeiculoNaoExistir()
    {
        var contexto = new FakeContextoOrdemServico();
        var cliente = CriarCliente();
        contexto.ClienteGateway.Clientes.Add(cliente);
        var useCase = contexto.CriarOrdemServicoUseCase();

        Assert.Throws<InvalidOperationException>(() => useCase.Executar(new CriarOrdemServicoInput(cliente.CpfCnpj, Guid.NewGuid())));
    }

    [Fact]
    public void Criar_DeveLancarExcecao_QuandoVeiculoNaoPertencerAoCliente()
    {
        var contexto = new FakeContextoOrdemServico();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(Guid.NewGuid());
        contexto.ClienteGateway.Clientes.Add(cliente);
        contexto.VeiculoGateway.Veiculos.Add(veiculo);
        var useCase = contexto.CriarOrdemServicoUseCase();

        Assert.Throws<InvalidOperationException>(() => useCase.Executar(new CriarOrdemServicoInput(cliente.CpfCnpj, veiculo.Id)));
    }

    [Fact]
    public void Criar_DeveIniciarComoRecebidaESemOrcamento()
    {
        var contexto = CriarContextoBasico();
        var useCase = contexto.CriarOrdemServicoUseCase();

        var response = useCase.Executar(new CriarOrdemServicoInput(contexto.Cliente.CpfCnpj, contexto.Veiculo.Id));

        Assert.Equal(StatusOrdemServico.Recebida, response.Status);
        Assert.Equal(StatusAprovacaoOrcamento.Pendente, response.StatusAprovacaoOrcamento);
        Assert.Equal(0m, response.ValorTotalServicos);
        Assert.Equal(0m, response.ValorTotalPecasInsumos);
        Assert.Equal(0m, response.ValorTotalOrcamento);
        Assert.Equal(string.Empty, response.EnvioOrcamento);
    }

    [Fact]
    public void Criar_DeveCriarClienteEVeiculo_QuandoEnviarCadastroCompleto()
    {
        var contexto = new FakeContextoOrdemServico();
        var useCase = contexto.CriarOrdemServicoUseCase();
        var input = new CriarOrdemServicoInput(
            null,
            null,
            new CriarOrdemServicoClienteInput("Ana Silva", "390.533.447-05", "ana@email.com", "11999999999"),
            new CriarOrdemServicoVeiculoInput("bra-2e19", "Honda", "Civic", 2023));

        var response = useCase.Executar(input);

        Assert.Equal(StatusOrdemServico.Recebida, response.Status);
        Assert.Single(contexto.ClienteGateway.Clientes);
        Assert.Single(contexto.VeiculoGateway.Veiculos);
        Assert.Equal("39053344705", contexto.ClienteGateway.Clientes[0].CpfCnpj);
        Assert.Equal("BRA2E19", contexto.VeiculoGateway.Veiculos[0].Placa);
        Assert.Equal(contexto.ClienteGateway.Clientes[0].Id, contexto.VeiculoGateway.Veiculos[0].ClienteId);
    }

    [Fact]
    public void Criar_DeveIncluirServicosEPecasInsumos_QuandoEnviarItensIniciais()
    {
        var contexto = CriarContextoBasico();
        var useCase = contexto.CriarOrdemServicoUseCase();
        var input = new CriarOrdemServicoInput(
            contexto.Cliente.CpfCnpj,
            contexto.Veiculo.Id,
            null,
            null,
            [contexto.Servico.Id],
            [new OrdemServicoItemPecaInsumoInput(contexto.Peca.Id, 2)]);

        var response = useCase.Executar(input);

        Assert.Single(response.ItensServico);
        Assert.Single(response.ItensPecaInsumo);
        Assert.Equal(150m, response.ValorTotalServicos);
        Assert.Equal(70m, response.ValorTotalPecasInsumos);
        Assert.Equal(220m, response.ValorTotalOrcamento);
        Assert.Equal(StatusOrdemServico.Recebida, response.Status);
        Assert.Equal(StatusAprovacaoOrcamento.Pendente, response.StatusAprovacaoOrcamento);
    }

    [Fact]
    public void Criar_DeveLancarExcecao_QuandoServicoInicialNaoExistir()
    {
        var contexto = CriarContextoBasico();
        var useCase = contexto.CriarOrdemServicoUseCase();
        var input = new CriarOrdemServicoInput(
            contexto.Cliente.CpfCnpj,
            contexto.Veiculo.Id,
            null,
            null,
            [Guid.NewGuid()],
            []);

        Assert.Throws<InvalidOperationException>(() => useCase.Executar(input));
    }

    [Fact]
    public void Criar_DeveLancarExcecao_QuandoMisturarModosDeCriacao()
    {
        var contexto = new FakeContextoOrdemServico();
        var useCase = contexto.CriarOrdemServicoUseCase();
        var input = new CriarOrdemServicoInput(
            "39053344705",
            Guid.NewGuid(),
            new CriarOrdemServicoClienteInput("Ana Silva", "390.533.447-05", "ana@email.com", "11999999999"),
            null);

        Assert.Throws<ValidacaoException>(() => useCase.Executar(input));
    }

    [Fact]
    public void Criar_DeveLancarExcecao_QuandoCadastroCompletoTiverCpfCnpjDuplicado()
    {
        var contexto = new FakeContextoOrdemServico();
        contexto.ClienteGateway.Clientes.Add(CriarCliente());
        var useCase = contexto.CriarOrdemServicoUseCase();
        var input = new CriarOrdemServicoInput(
            null,
            null,
            new CriarOrdemServicoClienteInput("Ana Silva", "390.533.447-05", "ana@email.com", "11999999999"),
            new CriarOrdemServicoVeiculoInput("BRA2E19", "Honda", "Civic", 2023));

        Assert.Throws<InvalidOperationException>(() => useCase.Executar(input));
    }

    [Fact]
    public void EnviarOrcamento_DeveAlterarStatusESimularEnvio()
    {
        var contexto = new FakeContextoOrdemServico();
        var cliente = CriarCliente();
        var servico = CriarServico();
        var peca = CriarPeca();
        var ordem = CriarOrdem(cliente, status: StatusOrdemServico.EmDiagnostico);
        contexto.OrdemServicoGateway.OrdensServico.Add(ordem);
        contexto.ServicoGateway.Servicos.Add(servico);
        contexto.PecaInsumoGateway.Pecas.Add(peca);
        var useCase = contexto.EnviarOrcamentoUseCase();

        var response = useCase.Executar(ordem.Id, new OrdemServicoOrcamentoInput(
            [servico.Id],
            [new OrdemServicoItemPecaInsumoInput(peca.Id, 2)]));

        Assert.NotNull(response);
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, response.Status);
        Assert.Equal(150m, response.ValorTotalServicos);
        Assert.Equal(70m, response.ValorTotalPecasInsumos);
        Assert.Equal(220m, response.ValorTotalOrcamento);
        Assert.Single(response.ItensServico);
        Assert.Single(response.ItensPecaInsumo);
        Assert.Contains("Orcamento enviado", response.EnvioOrcamento);
    }

    [Fact]
    public void EnviarOrcamento_DeveLancarExcecao_QuandoOrdemNaoEstiverEmDiagnostico()
    {
        var contexto = new FakeContextoOrdemServico();
        var ordem = new OrdemServico
        {
            Id = Guid.NewGuid(),
            Status = StatusOrdemServico.Recebida,
            StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Pendente
        };
        contexto.OrdemServicoGateway.OrdensServico.Add(ordem);
        var useCase = contexto.EnviarOrcamentoUseCase();

        Assert.Throws<InvalidOperationException>(() => useCase.Executar(ordem.Id, new OrdemServicoOrcamentoInput([], [])));
    }

    [Fact]
    public void AprovarOrcamento_DeveBaixarEstoqueEAlterarStatus()
    {
        var contexto = new FakeContextoOrdemServico();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var peca = CriarPeca();
        var ordem = CriarOrdemComPeca(cliente, veiculo, peca, StatusOrdemServico.AguardandoAprovacao);
        contexto.OrdemServicoGateway.OrdensServico.Add(ordem);
        contexto.PecaInsumoGateway.Pecas.Add(peca);
        var useCase = contexto.AprovarOrcamentoUseCase();

        var response = useCase.Executar(ordem.Id);

        Assert.NotNull(response);
        Assert.Equal(StatusOrdemServico.EmExecucao, response.Status);
        Assert.Equal(StatusAprovacaoOrcamento.Aprovado, response.StatusAprovacaoOrcamento);
        Assert.Equal(8, contexto.PecaInsumoGateway.Pecas[0].QuantidadeEstoque);
    }

    [Fact]
    public void AprovarOrcamento_DeveLancarExcecao_QuandoEstoqueForInsuficiente()
    {
        var contexto = new FakeContextoOrdemServico();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var peca = CriarPeca(quantidadeEstoque: 1);
        var ordem = CriarOrdemComPeca(cliente, veiculo, peca, StatusOrdemServico.AguardandoAprovacao);
        contexto.OrdemServicoGateway.OrdensServico.Add(ordem);
        contexto.PecaInsumoGateway.Pecas.Add(peca);
        var useCase = contexto.AprovarOrcamentoUseCase();

        Assert.Throws<InvalidOperationException>(() => useCase.Executar(ordem.Id));
    }

    [Fact]
    public void IniciarDiagnostico_DeveAlterarStatusParaEmDiagnostico()
    {
        var contexto = new FakeContextoOrdemServico();
        var ordem = new OrdemServico
        {
            Id = Guid.NewGuid(),
            Status = StatusOrdemServico.Recebida,
            StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Pendente
        };
        contexto.OrdemServicoGateway.OrdensServico.Add(ordem);
        var useCase = contexto.IniciarDiagnosticoUseCase();

        var response = useCase.Executar(ordem.Id);

        Assert.NotNull(response);
        Assert.Equal(StatusOrdemServico.EmDiagnostico, response.Status);
    }

    [Fact]
    public void RecusarOrcamento_DeveVoltarParaDiagnostico()
    {
        var contexto = new FakeContextoOrdemServico();
        var ordem = new OrdemServico
        {
            Id = Guid.NewGuid(),
            Status = StatusOrdemServico.AguardandoAprovacao,
            StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Pendente
        };
        contexto.OrdemServicoGateway.OrdensServico.Add(ordem);
        var useCase = contexto.RecusarOrcamentoUseCase();

        var response = useCase.Executar(ordem.Id, new RecusarOrcamentoInput("Nao aprovado"));

        Assert.NotNull(response);
        Assert.Equal(StatusOrdemServico.EmDiagnostico, response.Status);
        Assert.Equal(StatusAprovacaoOrcamento.Recusado, response.StatusAprovacaoOrcamento);
        Assert.Equal("Nao aprovado", response.MotivoRecusaOrcamento);
    }

    [Fact]
    public void Cancelar_DeveRestaurarEstoqueEAlterarStatus()
    {
        var contexto = new FakeContextoOrdemServico();
        var cliente = CriarCliente();
        var veiculo = CriarVeiculo(cliente.Id);
        var peca = CriarPeca(quantidadeEstoque: 8);
        var ordem = CriarOrdemComPeca(cliente, veiculo, peca, StatusOrdemServico.EmExecucao);
        ordem.StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Aprovado;
        contexto.OrdemServicoGateway.OrdensServico.Add(ordem);
        contexto.PecaInsumoGateway.Pecas.Add(peca);
        var useCase = contexto.CancelarOrdemServicoUseCase();

        var response = useCase.Executar(ordem.Id);

        Assert.NotNull(response);
        Assert.Equal(StatusOrdemServico.Cancelada, response.Status);
        Assert.Equal(10, contexto.PecaInsumoGateway.Pecas[0].QuantidadeEstoque);
    }

    [Fact]
    public void Cancelar_DeveLancarExcecao_QuandoOrdemJaEstiverFinalizada()
    {
        var contexto = new FakeContextoOrdemServico();
        var ordem = new OrdemServico
        {
            Id = Guid.NewGuid(),
            Status = StatusOrdemServico.Finalizada
        };
        contexto.OrdemServicoGateway.OrdensServico.Add(ordem);
        var useCase = contexto.CancelarOrdemServicoUseCase();

        Assert.Throws<InvalidOperationException>(() => useCase.Executar(ordem.Id));
    }

    [Fact]
    public void Finalizar_DeveLancarExcecao_QuandoOrdemNaoEstiverEmExecucao()
    {
        var contexto = new FakeContextoOrdemServico();
        var ordem = new OrdemServico
        {
            Id = Guid.NewGuid(),
            Status = StatusOrdemServico.AguardandoAprovacao
        };
        contexto.OrdemServicoGateway.OrdensServico.Add(ordem);
        var useCase = contexto.FinalizarOrdemServicoUseCase();

        Assert.Throws<InvalidOperationException>(() => useCase.Executar(ordem.Id));
    }

    [Fact]
    public void Finalizar_DeveAlterarStatusParaFinalizada()
    {
        var contexto = new FakeContextoOrdemServico();
        var ordem = new OrdemServico
        {
            Id = Guid.NewGuid(),
            Status = StatusOrdemServico.EmExecucao
        };
        contexto.OrdemServicoGateway.OrdensServico.Add(ordem);
        var useCase = contexto.FinalizarOrdemServicoUseCase();

        var response = useCase.Executar(ordem.Id);

        Assert.NotNull(response);
        Assert.Equal(StatusOrdemServico.Finalizada, response.Status);
        Assert.NotNull(response.FinalizadaEm);
    }

    [Fact]
    public void Entregar_DeveAlterarStatusParaEntregue()
    {
        var contexto = new FakeContextoOrdemServico();
        var ordem = new OrdemServico
        {
            Id = Guid.NewGuid(),
            Status = StatusOrdemServico.Finalizada
        };
        contexto.OrdemServicoGateway.OrdensServico.Add(ordem);
        var useCase = contexto.EntregarOrdemServicoUseCase();

        var response = useCase.Executar(ordem.Id);

        Assert.NotNull(response);
        Assert.Equal(StatusOrdemServico.Entregue, response.Status);
        Assert.NotNull(response.EntregueEm);
    }

    [Fact]
    public void ObterPorCpfCnpjCliente_DeveRetornarOrdensDoCliente()
    {
        var contexto = new FakeContextoOrdemServico();
        var cliente = CriarCliente();
        var outraOrdem = new OrdemServico
        {
            Id = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            Cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                CpfCnpj = "45513451808"
            }
        };
        var ordem = new OrdemServico
        {
            Id = Guid.NewGuid(),
            ClienteId = cliente.Id,
            Cliente = cliente
        };
        contexto.OrdemServicoGateway.OrdensServico.Add(ordem);
        contexto.OrdemServicoGateway.OrdensServico.Add(outraOrdem);
        var useCase = contexto.ObterOrdensPorCpfCnpjClienteUseCase();

        var response = useCase.Executar("390.533.447-05");

        Assert.Single(response);
        Assert.Equal(ordem.Id, response[0].Id);
    }

    [Fact]
    public void ObterTempoMedioExecucao_DeveCalcularMediaDasOrdensFinalizadas()
    {
        var contexto = new FakeContextoOrdemServico();
        var agora = DateTime.UtcNow;
        contexto.OrdemServicoGateway.OrdensServico.Add(new OrdemServico
        {
            Id = Guid.NewGuid(),
            ExecucaoIniciadaEm = agora,
            FinalizadaEm = agora.AddMinutes(30)
        });
        contexto.OrdemServicoGateway.OrdensServico.Add(new OrdemServico
        {
            Id = Guid.NewGuid(),
            ExecucaoIniciadaEm = agora,
            FinalizadaEm = agora.AddMinutes(90)
        });
        var useCase = contexto.ObterTempoMedioExecucaoUseCase();

        var response = useCase.Executar();

        Assert.Equal(2, response.QuantidadeOrdensConsideradas);
        Assert.Equal(60, response.TempoMedioExecucaoEmMinutos);
        Assert.Equal("01:00:00", response.TempoMedioExecucaoFormatado);
    }

    [Fact]
    public void ObterTempoMedioExecucao_DeveRetornarZero_QuandoNaoHouverOrdensFinalizadas()
    {
        var contexto = new FakeContextoOrdemServico();
        var useCase = contexto.ObterTempoMedioExecucaoUseCase();

        var response = useCase.Executar();

        Assert.Equal(0, response.QuantidadeOrdensConsideradas);
        Assert.Equal(0, response.TempoMedioExecucaoEmMinutos);
        Assert.Equal("00:00:00", response.TempoMedioExecucaoFormatado);
    }

    private static FakeContextoOrdemServico CriarContextoBasico()
    {
        var contexto = new FakeContextoOrdemServico();
        contexto.Cliente = CriarCliente();
        contexto.Veiculo = CriarVeiculo(contexto.Cliente.Id);
        contexto.Servico = CriarServico();
        contexto.Peca = CriarPeca();

        contexto.ClienteGateway.Clientes.Add(contexto.Cliente);
        contexto.VeiculoGateway.Veiculos.Add(contexto.Veiculo);
        contexto.ServicoGateway.Servicos.Add(contexto.Servico);
        contexto.PecaInsumoGateway.Pecas.Add(contexto.Peca);

        return contexto;
    }

    private static Cliente CriarCliente()
    {
        return new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Carlos Mendes",
            CpfCnpj = "39053344705",
            Email = "carlos@email.com",
            Telefone = "11999999999"
        };
    }

    private static Veiculo CriarVeiculo(Guid clienteId)
    {
        return new Veiculo
        {
            Id = Guid.NewGuid(),
            ClienteId = clienteId,
            Placa = "BRA2E19",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2022
        };
    }

    private static Servico CriarServico()
    {
        return new Servico
        {
            Id = Guid.NewGuid(),
            Nome = "Troca de oleo",
            Descricao = "Troca",
            Preco = 150m
        };
    }

    private static PecaInsumo CriarPeca(int quantidadeEstoque = 10)
    {
        return new PecaInsumo
        {
            Id = Guid.NewGuid(),
            Nome = "Filtro",
            Descricao = "Filtro de oleo",
            PrecoUnitario = 35m,
            QuantidadeEstoque = quantidadeEstoque
        };
    }

    private static OrdemServico CriarOrdem(Cliente cliente, StatusOrdemServico status)
    {
        return new OrdemServico
        {
            Id = Guid.NewGuid(),
            ClienteId = cliente.Id,
            Status = status,
            StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Pendente,
            Cliente = cliente
        };
    }

    private static OrdemServico CriarOrdemComPeca(Cliente cliente, Veiculo veiculo, PecaInsumo peca, StatusOrdemServico status)
    {
        return new OrdemServico
        {
            Id = Guid.NewGuid(),
            ClienteId = cliente.Id,
            VeiculoId = veiculo.Id,
            Status = status,
            StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Pendente,
            Cliente = cliente,
            Veiculo = veiculo,
            ItensPecaInsumo =
            [
                new OrdemServicoItemPecaInsumo
                {
                    Id = Guid.NewGuid(),
                    OrdemServicoId = Guid.NewGuid(),
                    PecaInsumoId = peca.Id,
                    NomePecaInsumo = peca.Nome,
                    PrecoUnitario = peca.PrecoUnitario,
                    Quantidade = 2,
                    Subtotal = 70m
                }
            ]
        };
    }

    private sealed class FakeContextoOrdemServico
    {
        public FakeOrdemServicoGateway OrdemServicoGateway { get; } = new();
        public FakeClienteGateway ClienteGateway { get; } = new();
        public FakeVeiculoGateway VeiculoGateway { get; } = new();
        public FakeServicoGateway ServicoGateway { get; } = new();
        public FakePecaInsumoGateway PecaInsumoGateway { get; } = new();
        public FakeCpfCnpjValidatorGateway CpfCnpjValidatorGateway { get; } = new();
        public FakePlacaVeiculoValidatorGateway PlacaVeiculoValidatorGateway { get; } = new();

        public Cliente Cliente { get; set; } = new();
        public Veiculo Veiculo { get; set; } = new();
        public Servico Servico { get; set; } = new();
        public PecaInsumo Peca { get; set; } = new();

        public CriarOrdemServicoUseCase CriarOrdemServicoUseCase()
        {
            return new CriarOrdemServicoUseCase(
                OrdemServicoGateway,
                ClienteGateway,
                VeiculoGateway,
                ServicoGateway,
                PecaInsumoGateway,
                CpfCnpjValidatorGateway,
                PlacaVeiculoValidatorGateway);
        }

        public EnviarOrcamentoUseCase EnviarOrcamentoUseCase()
        {
            return new EnviarOrcamentoUseCase(OrdemServicoGateway, ServicoGateway, PecaInsumoGateway);
        }

        public AprovarOrcamentoUseCase AprovarOrcamentoUseCase()
        {
            return new AprovarOrcamentoUseCase(OrdemServicoGateway, PecaInsumoGateway);
        }

        public IniciarDiagnosticoUseCase IniciarDiagnosticoUseCase()
        {
            return new IniciarDiagnosticoUseCase(OrdemServicoGateway);
        }

        public RecusarOrcamentoUseCase RecusarOrcamentoUseCase()
        {
            return new RecusarOrcamentoUseCase(OrdemServicoGateway);
        }

        public CancelarOrdemServicoUseCase CancelarOrdemServicoUseCase()
        {
            return new CancelarOrdemServicoUseCase(OrdemServicoGateway, PecaInsumoGateway);
        }

        public FinalizarOrdemServicoUseCase FinalizarOrdemServicoUseCase()
        {
            return new FinalizarOrdemServicoUseCase(OrdemServicoGateway);
        }

        public EntregarOrdemServicoUseCase EntregarOrdemServicoUseCase()
        {
            return new EntregarOrdemServicoUseCase(OrdemServicoGateway);
        }

        public ObterOrdensPorCpfCnpjClienteUseCase ObterOrdensPorCpfCnpjClienteUseCase()
        {
            return new ObterOrdensPorCpfCnpjClienteUseCase(OrdemServicoGateway, CpfCnpjValidatorGateway);
        }

        public ObterTempoMedioExecucaoUseCase ObterTempoMedioExecucaoUseCase()
        {
            return new ObterTempoMedioExecucaoUseCase(OrdemServicoGateway);
        }
    }

    private sealed class FakeOrdemServicoGateway : IOrdemServicoGateway
    {
        public List<OrdemServico> OrdensServico { get; } = [];

        public OrdemServico Adicionar(OrdemServico ordemServico)
        {
            OrdensServico.Add(ordemServico);
            return ordemServico;
        }

        public OrdemServico? Atualizar(OrdemServico ordemServico)
        {
            var existente = OrdensServico.FirstOrDefault(os => os.Id == ordemServico.Id);

            if (existente is null)
            {
                return null;
            }

            var indice = OrdensServico.IndexOf(existente);
            OrdensServico[indice] = ordemServico;
            return ordemServico;
        }

        public OrdemServico? ObterPorId(Guid id)
        {
            return OrdensServico.FirstOrDefault(os => os.Id == id);
        }

        public List<OrdemServico> ObterPorCpfCnpjCliente(string cpfCnpj)
        {
            return OrdensServico
                .Where(os => os.Cliente?.CpfCnpj == cpfCnpj)
                .ToList();
        }

        public List<OrdemServico> ObterTodas()
        {
            return OrdensServico;
        }
    }

    private sealed class FakeClienteGateway : IClienteGateway
    {
        public List<Cliente> Clientes { get; } = [];

        public Cliente Adicionar(Cliente cliente)
        {
            Clientes.Add(cliente);
            return cliente;
        }

        public Cliente? Atualizar(Cliente cliente)
        {
            return cliente;
        }

        public bool ExistePorCpfCnpj(string cpfCnpj)
        {
            return Clientes.Any(cliente => cliente.CpfCnpj == cpfCnpj);
        }

        public bool ExistePorCpfCnpjExcetoId(string cpfCnpj, Guid id)
        {
            return Clientes.Any(cliente => cliente.CpfCnpj == cpfCnpj && cliente.Id != id);
        }

        public Cliente? ObterPorCpfCnpj(string cpfCnpj)
        {
            return Clientes.FirstOrDefault(cliente => cliente.CpfCnpj == cpfCnpj);
        }

        public Cliente? ObterPorId(Guid id)
        {
            return Clientes.FirstOrDefault(cliente => cliente.Id == id);
        }

        public List<Cliente> ObterTodos()
        {
            return Clientes;
        }

        public bool Remover(Guid id)
        {
            return true;
        }
    }

    private sealed class FakeVeiculoGateway : IVeiculoGateway
    {
        public List<Veiculo> Veiculos { get; } = [];

        public Veiculo Adicionar(Veiculo veiculo)
        {
            Veiculos.Add(veiculo);
            return veiculo;
        }

        public Veiculo? Atualizar(Veiculo veiculo)
        {
            return veiculo;
        }

        public bool ClienteExiste(Guid clienteId)
        {
            return Veiculos.Any(veiculo => veiculo.ClienteId == clienteId);
        }

        public bool ExistePorPlaca(string placa)
        {
            return Veiculos.Any(veiculo => veiculo.Placa == placa);
        }

        public bool ExistePorPlacaExcetoId(string placa, Guid id)
        {
            return Veiculos.Any(veiculo => veiculo.Placa == placa && veiculo.Id != id);
        }

        public Veiculo? ObterPorId(Guid id)
        {
            return Veiculos.FirstOrDefault(veiculo => veiculo.Id == id);
        }

        public List<Veiculo> ObterTodos()
        {
            return Veiculos;
        }

        public bool Remover(Guid id)
        {
            return true;
        }
    }

    private sealed class FakeServicoGateway : IServicoGateway
    {
        public List<Servico> Servicos { get; } = [];

        public Servico Adicionar(Servico servico)
        {
            Servicos.Add(servico);
            return servico;
        }

        public Servico? Atualizar(Servico servico)
        {
            return servico;
        }

        public bool ExistePorNome(string nome)
        {
            return Servicos.Any(servico => servico.Nome == nome);
        }

        public bool ExistePorNomeExcetoId(string nome, Guid id)
        {
            return Servicos.Any(servico => servico.Nome == nome && servico.Id != id);
        }

        public Servico? ObterPorId(Guid id)
        {
            return Servicos.FirstOrDefault(servico => servico.Id == id);
        }

        public List<Servico> ObterTodos()
        {
            return Servicos;
        }

        public bool Remover(Guid id)
        {
            return true;
        }
    }

    private sealed class FakePecaInsumoGateway : IPecaInsumoGateway
    {
        public List<PecaInsumo> Pecas { get; } = [];

        public PecaInsumo Adicionar(PecaInsumo pecaInsumo)
        {
            Pecas.Add(pecaInsumo);
            return pecaInsumo;
        }

        public PecaInsumo? Atualizar(PecaInsumo pecaInsumo)
        {
            var existente = Pecas.FirstOrDefault(p => p.Id == pecaInsumo.Id);

            if (existente is null)
            {
                return null;
            }

            existente.Nome = pecaInsumo.Nome;
            existente.Descricao = pecaInsumo.Descricao;
            existente.PrecoUnitario = pecaInsumo.PrecoUnitario;
            existente.QuantidadeEstoque = pecaInsumo.QuantidadeEstoque;
            return existente;
        }

        public bool ExistePorNome(string nome)
        {
            return Pecas.Any(peca => peca.Nome == nome);
        }

        public bool ExistePorNomeExcetoId(string nome, Guid id)
        {
            return Pecas.Any(peca => peca.Nome == nome && peca.Id != id);
        }

        public PecaInsumo? ObterPorId(Guid id)
        {
            return Pecas.FirstOrDefault(peca => peca.Id == id);
        }

        public List<PecaInsumo> ObterTodos()
        {
            return Pecas;
        }

        public bool Remover(Guid id)
        {
            return true;
        }
    }
}

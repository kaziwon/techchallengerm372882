using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.Services;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.Domain.Repositories;

namespace OficinaMecanica.Api.UnitTests;

public class OrdemServicoServiceTests
{
    [Fact]
    public void Criar_DeveGerarOrcamentoEIniciarComoRecebida()
    {
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Carlos Mendes",
            CpfCnpj = "39053344705",
            Email = "carlos@email.com",
            Telefone = "11999999999"
        };
        var veiculo = new Veiculo
        {
            Id = Guid.NewGuid(),
            ClienteId = cliente.Id,
            Placa = "BRA2E19",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2022
        };
        var servico = new Servico
        {
            Id = Guid.NewGuid(),
            Nome = "Troca de oleo",
            Descricao = "Troca",
            Preco = 150m
        };
        var peca = new PecaInsumo
        {
            Id = Guid.NewGuid(),
            Nome = "Filtro",
            Descricao = "Filtro de oleo",
            PrecoUnitario = 35m,
            QuantidadeEstoque = 10
        };

        var service = CriarService(cliente, veiculo, servico, peca);

        var response = service.Criar(new OrdemServicoRequestDto
        {
            CpfCnpj = cliente.CpfCnpj,
            VeiculoId = veiculo.Id,
            ServicoIds = [servico.Id],
            PecasInsumos =
            [
                new OrdemServicoItemPecaInsumoRequestDto
                {
                    PecaInsumoId = peca.Id,
                    Quantidade = 2
                }
            ]
        });

        Assert.Equal(StatusOrdemServico.Recebida, response.Status);
        Assert.Equal(StatusAprovacaoOrcamento.Pendente, response.StatusAprovacaoOrcamento);
        Assert.Equal(150m, response.ValorTotalServicos);
        Assert.Equal(70m, response.ValorTotalPecasInsumos);
        Assert.Equal(220m, response.ValorTotalOrcamento);
        Assert.Equal(string.Empty, response.EnvioOrcamento);
    }

    [Fact]
    public void EnviarOrcamento_DeveAlterarStatusESimularEnvio()
    {
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Carlos Mendes",
            CpfCnpj = "39053344705",
            Email = "carlos@email.com"
        };
        var ordem = new OrdemServico
        {
            Id = Guid.NewGuid(),
            Status = StatusOrdemServico.EmDiagnostico,
            StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Pendente,
            Cliente = cliente
        };
        var ordemRepository = new FakeOrdemServicoRepository();
        ordemRepository.OrdensServico.Add(ordem);
        var service = new OrdemServicoService(
            ordemRepository,
            new FakeClienteRepositoryOrdem(),
            new FakeVeiculoRepositoryOrdem(),
            new FakeServicoRepositoryOrdem(),
            new FakePecaInsumoRepository());

        var response = service.EnviarOrcamento(ordem.Id);

        Assert.NotNull(response);
        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, response.Status);
        Assert.Contains("Orcamento enviado", response.EnvioOrcamento);
    }

    [Fact]
    public void AprovarOrcamento_DeveBaixarEstoqueEAlterarStatus()
    {
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Carlos Mendes",
            CpfCnpj = "39053344705",
            Email = "carlos@email.com"
        };
        var veiculo = new Veiculo
        {
            Id = Guid.NewGuid(),
            ClienteId = cliente.Id,
            Placa = "BRA2E19",
            Modelo = "Corolla"
        };
        var peca = new PecaInsumo
        {
            Id = Guid.NewGuid(),
            Nome = "Filtro",
            PrecoUnitario = 35m,
            QuantidadeEstoque = 10
        };
        var ordem = new OrdemServico
        {
            Id = Guid.NewGuid(),
            ClienteId = cliente.Id,
            VeiculoId = veiculo.Id,
            Status = StatusOrdemServico.AguardandoAprovacao,
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

        var ordemRepository = new FakeOrdemServicoRepository();
        ordemRepository.OrdensServico.Add(ordem);
        var pecaRepository = new FakePecaInsumoRepository();
        pecaRepository.Pecas.Add(peca);
        var service = new OrdemServicoService(
            ordemRepository,
            new FakeClienteRepositoryOrdem(),
            new FakeVeiculoRepositoryOrdem(),
            new FakeServicoRepositoryOrdem(),
            pecaRepository);

        var response = service.AprovarOrcamento(ordem.Id);

        Assert.NotNull(response);
        Assert.Equal(StatusOrdemServico.EmExecucao, response.Status);
        Assert.Equal(StatusAprovacaoOrcamento.Aprovado, response.StatusAprovacaoOrcamento);
        Assert.Equal(8, pecaRepository.Pecas[0].QuantidadeEstoque);
    }

    [Fact]
    public void IniciarDiagnostico_DeveAlterarStatusParaEmDiagnostico()
    {
        var ordem = new OrdemServico
        {
            Id = Guid.NewGuid(),
            Status = StatusOrdemServico.Recebida,
            StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Pendente
        };
        var ordemRepository = new FakeOrdemServicoRepository();
        ordemRepository.OrdensServico.Add(ordem);
        var service = new OrdemServicoService(
            ordemRepository,
            new FakeClienteRepositoryOrdem(),
            new FakeVeiculoRepositoryOrdem(),
            new FakeServicoRepositoryOrdem(),
            new FakePecaInsumoRepository());

        var response = service.IniciarDiagnostico(ordem.Id);

        Assert.NotNull(response);
        Assert.Equal(StatusOrdemServico.EmDiagnostico, response.Status);
    }

    [Fact]
    public void RecusarOrcamento_DeveVoltarParaDiagnostico()
    {
        var ordem = new OrdemServico
        {
            Id = Guid.NewGuid(),
            Status = StatusOrdemServico.AguardandoAprovacao,
            StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Pendente
        };
        var ordemRepository = new FakeOrdemServicoRepository();
        ordemRepository.OrdensServico.Add(ordem);
        var service = new OrdemServicoService(
            ordemRepository,
            new FakeClienteRepositoryOrdem(),
            new FakeVeiculoRepositoryOrdem(),
            new FakeServicoRepositoryOrdem(),
            new FakePecaInsumoRepository());

        var response = service.RecusarOrcamento(ordem.Id, new OrdemServicoRespostaAprovacaoRequestDto
        {
            MotivoRecusa = "Nao aprovado"
        });

        Assert.NotNull(response);
        Assert.Equal(StatusOrdemServico.EmDiagnostico, response.Status);
        Assert.Equal(StatusAprovacaoOrcamento.Recusado, response.StatusAprovacaoOrcamento);
        Assert.Equal("Nao aprovado", response.MotivoRecusaOrcamento);
    }

    [Fact]
    public void Cancelar_DeveRestaurarEstoqueEAlterarStatus()
    {
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Carlos Mendes",
            CpfCnpj = "39053344705",
            Email = "carlos@email.com"
        };
        var veiculo = new Veiculo
        {
            Id = Guid.NewGuid(),
            ClienteId = cliente.Id,
            Placa = "BRA2E19",
            Modelo = "Corolla"
        };
        var peca = new PecaInsumo
        {
            Id = Guid.NewGuid(),
            Nome = "Filtro",
            PrecoUnitario = 35m,
            QuantidadeEstoque = 8
        };
        var ordem = new OrdemServico
        {
            Id = Guid.NewGuid(),
            ClienteId = cliente.Id,
            VeiculoId = veiculo.Id,
            Status = StatusOrdemServico.EmExecucao,
            StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Aprovado,
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

        var ordemRepository = new FakeOrdemServicoRepository();
        ordemRepository.OrdensServico.Add(ordem);
        var pecaRepository = new FakePecaInsumoRepository();
        pecaRepository.Pecas.Add(peca);
        var service = new OrdemServicoService(
            ordemRepository,
            new FakeClienteRepositoryOrdem(),
            new FakeVeiculoRepositoryOrdem(),
            new FakeServicoRepositoryOrdem(),
            pecaRepository);

        var response = service.Cancelar(ordem.Id);

        Assert.NotNull(response);
        Assert.Equal(StatusOrdemServico.Cancelada, response.Status);
        Assert.Equal(10, pecaRepository.Pecas[0].QuantidadeEstoque);
    }

    [Fact]
    public void Finalizar_DeveLancarExcecao_QuandoOrdemNaoEstiverEmExecucao()
    {
        var ordem = new OrdemServico
        {
            Id = Guid.NewGuid(),
            Status = StatusOrdemServico.AguardandoAprovacao
        };
        var ordemRepository = new FakeOrdemServicoRepository();
        ordemRepository.OrdensServico.Add(ordem);
        var service = new OrdemServicoService(
            ordemRepository,
            new FakeClienteRepositoryOrdem(),
            new FakeVeiculoRepositoryOrdem(),
            new FakeServicoRepositoryOrdem(),
            new FakePecaInsumoRepository());

        Assert.Throws<InvalidOperationException>(() => service.Finalizar(ordem.Id));
    }

    [Fact]
    public void ObterPorCpfCnpjCliente_DeveRetornarOrdensDoCliente()
    {
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Carlos Mendes",
            CpfCnpj = "39053344705"
        };
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
        var ordemRepository = new FakeOrdemServicoRepository();
        ordemRepository.OrdensServico.Add(ordem);
        ordemRepository.OrdensServico.Add(outraOrdem);
        var service = new OrdemServicoService(
            ordemRepository,
            new FakeClienteRepositoryOrdem(),
            new FakeVeiculoRepositoryOrdem(),
            new FakeServicoRepositoryOrdem(),
            new FakePecaInsumoRepository());

        var response = service.ObterPorCpfCnpjCliente("390.533.447-05");

        Assert.Single(response);
        Assert.Equal(ordem.Id, response[0].Id);
    }

    private static OrdemServicoService CriarService(Cliente cliente, Veiculo veiculo, Servico servico, PecaInsumo peca)
    {
        var clienteRepository = new FakeClienteRepositoryOrdem();
        clienteRepository.Clientes.Add(cliente);

        var veiculoRepository = new FakeVeiculoRepositoryOrdem();
        veiculoRepository.Veiculos.Add(veiculo);

        var servicoRepository = new FakeServicoRepositoryOrdem();
        servicoRepository.Servicos.Add(servico);

        var pecaRepository = new FakePecaInsumoRepository();
        pecaRepository.Pecas.Add(peca);

        return new OrdemServicoService(
            new FakeOrdemServicoRepository(),
            clienteRepository,
            veiculoRepository,
            servicoRepository,
            pecaRepository);
    }

    private sealed class FakeOrdemServicoRepository : IOrdemServicoRepository
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

    private sealed class FakeClienteRepositoryOrdem : IClienteRepository
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

    private sealed class FakeVeiculoRepositoryOrdem : IVeiculoRepository
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

    private sealed class FakeServicoRepositoryOrdem : IServicoRepository
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

    private sealed class FakePecaInsumoRepository : IPecaInsumoRepository
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

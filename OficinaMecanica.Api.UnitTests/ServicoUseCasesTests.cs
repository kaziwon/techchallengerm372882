using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Application.UseCases.Servicos;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.UnitTests;

public class ServicoUseCasesTests
{
    [Fact]
    public void Criar_DeveLancarExcecao_QuandoNomeJaExistir()
    {
        var gateway = new FakeServicoGateway
        {
            ExistePorNomeResult = true
        };
        var useCase = new CriarServicoUseCase(gateway);

        Assert.Throws<InvalidOperationException>(() => useCase.Executar(CriarInput()));
    }

    [Fact]
    public void Criar_DeveRetornarServicoCriado()
    {
        var gateway = new FakeServicoGateway();
        var useCase = new CriarServicoUseCase(gateway);

        var output = useCase.Executar(CriarInput());

        Assert.Equal("Troca de oleo", output.Nome);
        Assert.Equal(150m, output.Preco);
        Assert.Single(gateway.Servicos);
    }

    [Fact]
    public void Atualizar_DeveRetornarNull_QuandoServicoNaoExistir()
    {
        var gateway = new FakeServicoGateway();
        var useCase = new AtualizarServicoUseCase(gateway);

        var output = useCase.Executar(Guid.NewGuid(), CriarInput());

        Assert.Null(output);
    }

    private static ServicoInput CriarInput()
    {
        return new ServicoInput("Troca de oleo", "Troca completa", 150m);
    }

    private sealed class FakeServicoGateway : IServicoGateway
    {
        public List<Servico> Servicos { get; } = [];
        public bool ExistePorNomeResult { get; set; }
        public bool ExistePorNomeExcetoIdResult { get; set; }

        public Servico Adicionar(Servico servico)
        {
            Servicos.Add(servico);
            return servico;
        }

        public Servico? Atualizar(Servico servico)
        {
            var existente = Servicos.FirstOrDefault(s => s.Id == servico.Id);

            if (existente is null)
            {
                return null;
            }

            existente.Nome = servico.Nome;
            existente.Descricao = servico.Descricao;
            existente.Preco = servico.Preco;
            return existente;
        }

        public bool ExistePorNome(string nome)
        {
            return ExistePorNomeResult || Servicos.Any(servico => servico.Nome == nome);
        }

        public bool ExistePorNomeExcetoId(string nome, Guid id)
        {
            return ExistePorNomeExcetoIdResult || Servicos.Any(servico => servico.Nome == nome && servico.Id != id);
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
}

using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Application.UseCases.PecasInsumos;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.UnitTests;

public class PecaInsumoUseCasesTests
{
    [Fact]
    public void Criar_DeveLancarExcecao_QuandoNomeJaExistir()
    {
        var gateway = new FakePecaInsumoGateway
        {
            ExistePorNomeResult = true
        };
        var useCase = new CriarPecaInsumoUseCase(gateway);

        Assert.Throws<InvalidOperationException>(() => useCase.Executar(CriarInput()));
    }

    [Fact]
    public void Criar_DeveRetornarPecaCriada()
    {
        var gateway = new FakePecaInsumoGateway();
        var useCase = new CriarPecaInsumoUseCase(gateway);

        var output = useCase.Executar(CriarInput());

        Assert.Equal("Filtro de oleo", output.Nome);
        Assert.Equal(10, output.QuantidadeEstoque);
        Assert.Single(gateway.Pecas);
    }

    [Fact]
    public void Atualizar_DeveRetornarNull_QuandoPecaNaoExistir()
    {
        var gateway = new FakePecaInsumoGateway();
        var useCase = new AtualizarPecaInsumoUseCase(gateway);

        var output = useCase.Executar(Guid.NewGuid(), CriarInput());

        Assert.Null(output);
    }

    private static PecaInsumoInput CriarInput()
    {
        return new PecaInsumoInput("Filtro de oleo", "Filtro compativel", 35.9m, 10);
    }

    private sealed class FakePecaInsumoGateway : IPecaInsumoGateway
    {
        public List<PecaInsumo> Pecas { get; } = [];
        public bool ExistePorNomeResult { get; set; }
        public bool ExistePorNomeExcetoIdResult { get; set; }

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
            return ExistePorNomeResult || Pecas.Any(peca => peca.Nome == nome);
        }

        public bool ExistePorNomeExcetoId(string nome, Guid id)
        {
            return ExistePorNomeExcetoIdResult || Pecas.Any(peca => peca.Nome == nome && peca.Id != id);
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

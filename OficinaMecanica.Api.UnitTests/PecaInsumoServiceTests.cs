using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.Services;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.Domain.Repositories;

namespace OficinaMecanica.Api.UnitTests;

public class PecaInsumoServiceTests
{
    [Fact]
    public void Adicionar_DeveLancarExcecao_QuandoNomeJaExistir()
    {
        var repository = new FakePecaInsumoRepository
        {
            ExistePorNomeResult = true
        };
        var service = new PecaInsumoService(repository);

        Assert.Throws<InvalidOperationException>(() => service.Adicionar(CriarRequest()));
    }

    [Fact]
    public void Adicionar_DeveRetornarPecaCriada()
    {
        var repository = new FakePecaInsumoRepository();
        var service = new PecaInsumoService(repository);

        var response = service.Adicionar(CriarRequest());

        Assert.Equal("Filtro de oleo", response.Nome);
        Assert.Equal(10, response.QuantidadeEstoque);
        Assert.Single(repository.Pecas);
    }

    [Fact]
    public void Atualizar_DeveRetornarNull_QuandoPecaNaoExistir()
    {
        var repository = new FakePecaInsumoRepository();
        var service = new PecaInsumoService(repository);

        var response = service.Atualizar(Guid.NewGuid(), CriarRequest());

        Assert.Null(response);
    }

    private static PecaInsumoRequestDto CriarRequest()
    {
        return new PecaInsumoRequestDto
        {
            Nome = "Filtro de oleo",
            Descricao = "Filtro compativel",
            PrecoUnitario = 35.9m,
            QuantidadeEstoque = 10
        };
    }

    private sealed class FakePecaInsumoRepository : IPecaInsumoRepository
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

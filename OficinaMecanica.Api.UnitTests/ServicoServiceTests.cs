using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.Services;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.Domain.Repositories;

namespace OficinaMecanica.Api.UnitTests;

public class ServicoServiceTests
{
    [Fact]
    public void Adicionar_DeveLancarExcecao_QuandoNomeJaExistir()
    {
        var repository = new FakeServicoRepository
        {
            ExistePorNomeResult = true
        };
        var service = new ServicoService(repository);

        Assert.Throws<InvalidOperationException>(() => service.Adicionar(CriarRequest()));
    }

    [Fact]
    public void Adicionar_DeveRetornarServicoCriado()
    {
        var repository = new FakeServicoRepository();
        var service = new ServicoService(repository);

        var response = service.Adicionar(CriarRequest());

        Assert.Equal("Troca de oleo", response.Nome);
        Assert.Equal(150m, response.Preco);
        Assert.Single(repository.Servicos);
    }

    [Fact]
    public void Atualizar_DeveRetornarNull_QuandoServicoNaoExistir()
    {
        var repository = new FakeServicoRepository();
        var service = new ServicoService(repository);

        var response = service.Atualizar(Guid.NewGuid(), CriarRequest());

        Assert.Null(response);
    }

    private static ServicoRequestDto CriarRequest()
    {
        return new ServicoRequestDto
        {
            Nome = "Troca de oleo",
            Descricao = "Troca completa",
            Preco = 150m
        };
    }

    private sealed class FakeServicoRepository : IServicoRepository
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

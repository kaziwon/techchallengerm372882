using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.Services;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.Domain.Repositories;

namespace OficinaMecanica.Api.UnitTests;

public class ClienteServiceTests
{
    [Fact]
    public void Adicionar_DeveLancarExcecao_QuandoCpfCnpjJaExiste()
    {
        var repository = new FakeClienteRepository
        {
            ExistePorCpfCnpjResult = true
        };
        var service = new ClienteService(repository);
        var request = CriarRequest();

        Assert.Throws<InvalidOperationException>(() => service.Adicionar(request));
    }

    [Fact]
    public void Adicionar_DeveRetornarClienteResponseDto_QuandoDadosForemValidos()
    {
        var repository = new FakeClienteRepository();
        var service = new ClienteService(repository);
        var request = CriarRequest();

        var response = service.Adicionar(request);

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(request.Nome, response.Nome);
        Assert.Equal(request.CpfCnpj, response.CpfCnpj);
        Assert.Equal(request.Email, response.Email);
        Assert.Equal(request.Telefone, response.Telefone);
        Assert.Single(repository.Clientes);
    }

    [Fact]
    public void Atualizar_DeveLancarExcecao_QuandoCpfCnpjJaExisteParaOutroCliente()
    {
        var repository = new FakeClienteRepository
        {
            ExistePorCpfCnpjExcetoIdResult = true
        };
        var service = new ClienteService(repository);

        Assert.Throws<InvalidOperationException>(() => service.Atualizar(Guid.NewGuid(), CriarRequest()));
    }

    [Fact]
    public void Atualizar_DeveRetornarNull_QuandoClienteNaoExistir()
    {
        var repository = new FakeClienteRepository();
        var service = new ClienteService(repository);

        var response = service.Atualizar(Guid.NewGuid(), CriarRequest());

        Assert.Null(response);
    }

    [Fact]
    public void ObterTodos_DeveMapearClientesParaResponseDto()
    {
        var repository = new FakeClienteRepository();
        repository.Clientes.Add(new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Maria Oliveira",
            CpfCnpj = "11222333000181",
            Email = "maria@email.com",
            Telefone = "11999999999"
        });
        var service = new ClienteService(repository);

        var response = service.ObterTodos();

        Assert.Single(response);
        Assert.Equal("Maria Oliveira", response[0].Nome);
    }

    [Fact]
    public void Remover_DeveRetornarResultadoDoRepositorio()
    {
        var id = Guid.NewGuid();
        var repository = new FakeClienteRepository();
        repository.Clientes.Add(new Cliente
        {
            Id = id,
            Nome = "Maria Oliveira",
            CpfCnpj = "11222333000181",
            Email = "maria@email.com",
            Telefone = "11999999999"
        });
        var service = new ClienteService(repository);

        var removido = service.Remover(id);

        Assert.True(removido);
        Assert.Empty(repository.Clientes);
    }

    private static ClienteRequestDto CriarRequest()
    {
        return new ClienteRequestDto
        {
            Nome = "Joao Silva",
            CpfCnpj = "52998224725",
            Email = "joao@email.com",
            Telefone = "11999999999"
        };
    }

    private sealed class FakeClienteRepository : IClienteRepository
    {
        public List<Cliente> Clientes { get; } = [];
        public bool ExistePorCpfCnpjResult { get; set; }
        public bool ExistePorCpfCnpjExcetoIdResult { get; set; }

        public Cliente Adicionar(Cliente cliente)
        {
            Clientes.Add(cliente);
            return cliente;
        }

        public Cliente? Atualizar(Cliente cliente)
        {
            var clienteExistente = Clientes.FirstOrDefault(c => c.Id == cliente.Id);

            if (clienteExistente is null)
            {
                return null;
            }

            clienteExistente.Nome = cliente.Nome;
            clienteExistente.CpfCnpj = cliente.CpfCnpj;
            clienteExistente.Email = cliente.Email;
            clienteExistente.Telefone = cliente.Telefone;

            return clienteExistente;
        }

        public bool ExistePorCpfCnpj(string cpfCnpj)
        {
            return ExistePorCpfCnpjResult || Clientes.Any(cliente => cliente.CpfCnpj == cpfCnpj);
        }

        public Cliente? ObterPorCpfCnpj(string cpfCnpj)
        {
            return Clientes.FirstOrDefault(cliente => cliente.CpfCnpj == cpfCnpj);
        }

        public bool ExistePorCpfCnpjExcetoId(string cpfCnpj, Guid id)
        {
            return ExistePorCpfCnpjExcetoIdResult || Clientes.Any(cliente => cliente.CpfCnpj == cpfCnpj && cliente.Id != id);
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
            var cliente = Clientes.FirstOrDefault(c => c.Id == id);

            if (cliente is null)
            {
                return false;
            }

            Clientes.Remove(cliente);
            return true;
        }
    }
}

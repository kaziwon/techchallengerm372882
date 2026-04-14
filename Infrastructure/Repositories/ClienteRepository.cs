using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.Domain.Repositories;

namespace OficinaMecanica.Api.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private static readonly List<Cliente> Clientes =
    [
        new Cliente
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Nome = "Joao Silva",
            CpfCnpj = "12345678901",
            Email = "joao.silva@email.com",
            Telefone = "11999999999"
        }
    ];

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

    public List<Cliente> ObterTodos()
    {
        return Clientes;
    }

    public Cliente? ObterPorId(Guid id)
    {
        return Clientes.FirstOrDefault(cliente => cliente.Id == id);
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

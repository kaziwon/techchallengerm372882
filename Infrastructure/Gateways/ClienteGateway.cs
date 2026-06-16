using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.Infrastructure.Persistence;

namespace OficinaMecanica.Api.Infrastructure.Gateways;

public class ClienteGateway : IClienteGateway
{
    private readonly AppDbContext _context;

    public ClienteGateway(AppDbContext context)
    {
        _context = context;
    }

    public Cliente Adicionar(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        _context.SaveChanges();

        return cliente;
    }

    public Cliente? Atualizar(Cliente cliente)
    {
        var clienteExistente = _context.Clientes.FirstOrDefault(c => c.Id == cliente.Id);

        if (clienteExistente is null)
        {
            return null;
        }

        clienteExistente.Nome = cliente.Nome;
        clienteExistente.CpfCnpj = cliente.CpfCnpj;
        clienteExistente.Email = cliente.Email;
        clienteExistente.Telefone = cliente.Telefone;

        _context.SaveChanges();

        return clienteExistente;
    }

    public bool ExistePorCpfCnpj(string cpfCnpj)
    {
        return _context.Clientes.Any(cliente => cliente.CpfCnpj == cpfCnpj);
    }

    public bool ExistePorCpfCnpjExcetoId(string cpfCnpj, Guid id)
    {
        return _context.Clientes.Any(cliente => cliente.CpfCnpj == cpfCnpj && cliente.Id != id);
    }

    public Cliente? ObterPorId(Guid id)
    {
        return _context.Clientes.AsNoTracking().FirstOrDefault(cliente => cliente.Id == id);
    }

    public Cliente? ObterPorCpfCnpj(string cpfCnpj)
    {
        return _context.Clientes.AsNoTracking().FirstOrDefault(cliente => cliente.CpfCnpj == cpfCnpj);
    }

    public List<Cliente> ObterTodos()
    {
        return _context.Clientes.AsNoTracking().ToList();
    }

    public bool Remover(Guid id)
    {
        var cliente = _context.Clientes.FirstOrDefault(c => c.Id == id);

        if (cliente is null)
        {
            return false;
        }

        _context.Clientes.Remove(cliente);
        _context.SaveChanges();

        return true;
    }
}

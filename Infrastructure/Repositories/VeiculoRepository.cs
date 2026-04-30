using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.Domain.Repositories;
using OficinaMecanica.Api.Infrastructure.Persistence;

namespace OficinaMecanica.Api.Infrastructure.Repositories;

public class VeiculoRepository : IVeiculoRepository
{
    private readonly AppDbContext _context;

    public VeiculoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Veiculo Adicionar(Veiculo veiculo)
    {
        _context.Veiculos.Add(veiculo);
        _context.SaveChanges();

        return veiculo;
    }

    public Veiculo? Atualizar(Veiculo veiculo)
    {
        var veiculoExistente = _context.Veiculos.FirstOrDefault(v => v.Id == veiculo.Id);

        if (veiculoExistente is null)
        {
            return null;
        }

        veiculoExistente.ClienteId = veiculo.ClienteId;
        veiculoExistente.Placa = veiculo.Placa;
        veiculoExistente.Marca = veiculo.Marca;
        veiculoExistente.Modelo = veiculo.Modelo;
        veiculoExistente.Ano = veiculo.Ano;

        _context.SaveChanges();

        return veiculoExistente;
    }

    public bool ClienteExiste(Guid clienteId)
    {
        return _context.Clientes.Any(cliente => cliente.Id == clienteId);
    }

    public bool ExistePorPlaca(string placa)
    {
        return _context.Veiculos.Any(veiculo => veiculo.Placa == placa.Trim().ToUpper().Replace("-", ""));
    }

    public bool ExistePorPlacaExcetoId(string placa, Guid id)
    {
        var placaNormalizada = placa.Trim().ToUpper().Replace("-", "");
        return _context.Veiculos.Any(veiculo => veiculo.Placa == placaNormalizada && veiculo.Id != id);
    }

    public Veiculo? ObterPorId(Guid id)
    {
        return _context.Veiculos.AsNoTracking().FirstOrDefault(veiculo => veiculo.Id == id);
    }

    public List<Veiculo> ObterTodos()
    {
        return _context.Veiculos.AsNoTracking().ToList();
    }

    public bool Remover(Guid id)
    {
        var veiculo = _context.Veiculos.FirstOrDefault(v => v.Id == id);

        if (veiculo is null)
        {
            return false;
        }

        _context.Veiculos.Remove(veiculo);
        _context.SaveChanges();

        return true;
    }
}

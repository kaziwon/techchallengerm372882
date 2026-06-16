using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.Infrastructure.Persistence;

namespace OficinaMecanica.Api.Infrastructure.Gateways;

public class PecaInsumoGateway : IPecaInsumoGateway
{
    private readonly AppDbContext _context;

    public PecaInsumoGateway(AppDbContext context)
    {
        _context = context;
    }

    public PecaInsumo Adicionar(PecaInsumo pecaInsumo)
    {
        _context.PecasInsumos.Add(pecaInsumo);
        _context.SaveChanges();

        return pecaInsumo;
    }

    public PecaInsumo? Atualizar(PecaInsumo pecaInsumo)
    {
        var pecaInsumoExistente = _context.PecasInsumos.FirstOrDefault(p => p.Id == pecaInsumo.Id);

        if (pecaInsumoExistente is null)
        {
            return null;
        }

        pecaInsumoExistente.Nome = pecaInsumo.Nome;
        pecaInsumoExistente.Descricao = pecaInsumo.Descricao;
        pecaInsumoExistente.PrecoUnitario = pecaInsumo.PrecoUnitario;
        pecaInsumoExistente.QuantidadeEstoque = pecaInsumo.QuantidadeEstoque;

        _context.SaveChanges();

        return pecaInsumoExistente;
    }

    public bool ExistePorNome(string nome)
    {
        return _context.PecasInsumos.Any(pecaInsumo => pecaInsumo.Nome == nome);
    }

    public bool ExistePorNomeExcetoId(string nome, Guid id)
    {
        return _context.PecasInsumos.Any(pecaInsumo => pecaInsumo.Nome == nome && pecaInsumo.Id != id);
    }

    public PecaInsumo? ObterPorId(Guid id)
    {
        return _context.PecasInsumos.AsNoTracking().FirstOrDefault(pecaInsumo => pecaInsumo.Id == id);
    }

    public List<PecaInsumo> ObterTodos()
    {
        return _context.PecasInsumos.AsNoTracking().ToList();
    }

    public bool Remover(Guid id)
    {
        var pecaInsumo = _context.PecasInsumos.FirstOrDefault(p => p.Id == id);

        if (pecaInsumo is null)
        {
            return false;
        }

        _context.PecasInsumos.Remove(pecaInsumo);
        _context.SaveChanges();

        return true;
    }
}

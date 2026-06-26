using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.Infrastructure.Persistence;
using OficinaMecanica.Api.InterfaceAdapters.DataSources;

namespace OficinaMecanica.Api.Infrastructure.Sources;

public class EfServicoDataSource : IServicoDataSource
{
    private readonly AppDbContext _context;

    public EfServicoDataSource(AppDbContext context)
    {
        _context = context;
    }

    public Servico Adicionar(Servico servico)
    {
        _context.Servicos.Add(servico);
        _context.SaveChanges();

        return servico;
    }

    public Servico? Atualizar(Servico servico)
    {
        var servicoExistente = _context.Servicos.FirstOrDefault(s => s.Id == servico.Id);

        if (servicoExistente is null)
        {
            return null;
        }

        servicoExistente.Nome = servico.Nome;
        servicoExistente.Descricao = servico.Descricao;
        servicoExistente.Preco = servico.Preco;

        _context.SaveChanges();

        return servicoExistente;
    }

    public bool ExistePorNome(string nome)
    {
        return _context.Servicos.Any(servico => servico.Nome == nome);
    }

    public bool ExistePorNomeExcetoId(string nome, Guid id)
    {
        return _context.Servicos.Any(servico => servico.Nome == nome && servico.Id != id);
    }

    public Servico? ObterPorId(Guid id)
    {
        return _context.Servicos.AsNoTracking().FirstOrDefault(servico => servico.Id == id);
    }

    public List<Servico> ObterTodos()
    {
        return _context.Servicos.AsNoTracking().ToList();
    }

    public bool Remover(Guid id)
    {
        var servico = _context.Servicos.FirstOrDefault(s => s.Id == id);

        if (servico is null)
        {
            return false;
        }

        _context.Servicos.Remove(servico);
        _context.SaveChanges();

        return true;
    }
}

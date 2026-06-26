using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.Infrastructure.Persistence;
using OficinaMecanica.Api.InterfaceAdapters.DataSources;

namespace OficinaMecanica.Api.Infrastructure.Sources;

public class EfOrdemServicoDataSource : IOrdemServicoDataSource
{
    private readonly AppDbContext _context;

    public EfOrdemServicoDataSource(AppDbContext context)
    {
        _context = context;
    }

    public List<OrdemServico> ObterTodas()
    {
        return QueryCompleta().AsNoTracking().ToList();
    }

    public List<OrdemServico> ObterPorCpfCnpjCliente(string cpfCnpj)
    {
        return QueryCompleta()
            .AsNoTracking()
            .Where(ordemServico => ordemServico.Cliente != null && ordemServico.Cliente.CpfCnpj == cpfCnpj)
            .ToList();
    }

    public OrdemServico? ObterPorId(Guid id)
    {
        return QueryCompleta().FirstOrDefault(ordemServico => ordemServico.Id == id);
    }

    public OrdemServico Adicionar(OrdemServico ordemServico)
    {
        _context.OrdensServico.Add(ordemServico);
        _context.SaveChanges();

        return ObterPorId(ordemServico.Id)!;
    }

    public OrdemServico? Atualizar(OrdemServico ordemServico)
    {
        var ordemServicoExistente = _context.OrdensServico
            .Include(os => os.ItensServico)
            .Include(os => os.ItensPecaInsumo)
            .FirstOrDefault(os => os.Id == ordemServico.Id);

        if (ordemServicoExistente is null)
        {
            return null;
        }

        foreach (var itemServico in ordemServicoExistente.ItensServico)
        {
            var itemExiste = _context.OrdemServicoItensServico
                .AsNoTracking()
                .Any(item => item.Id == itemServico.Id);

            if (!itemExiste)
            {
                _context.Entry(itemServico).State = EntityState.Added;
            }
        }

        foreach (var itemPecaInsumo in ordemServicoExistente.ItensPecaInsumo)
        {
            var itemExiste = _context.OrdemServicoItensPecaInsumo
                .AsNoTracking()
                .Any(item => item.Id == itemPecaInsumo.Id);

            if (!itemExiste)
            {
                _context.Entry(itemPecaInsumo).State = EntityState.Added;
            }
        }

        _context.SaveChanges();

        return ObterPorId(ordemServico.Id);
    }

    private IQueryable<OrdemServico> QueryCompleta()
    {
        return _context.OrdensServico
            .Include(ordemServico => ordemServico.Cliente)
            .Include(ordemServico => ordemServico.Veiculo)
            .Include(ordemServico => ordemServico.ItensServico)
            .Include(ordemServico => ordemServico.ItensPecaInsumo);
    }
}

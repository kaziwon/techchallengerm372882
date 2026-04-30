using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.Domain.Repositories;
using OficinaMecanica.Api.Infrastructure.Persistence;

namespace OficinaMecanica.Api.Infrastructure.Repositories;

public class OrdemServicoRepository : IOrdemServicoRepository
{
    private readonly AppDbContext _context;

    public OrdemServicoRepository(AppDbContext context)
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
        var ordemServicoExistente = _context.OrdensServico.FirstOrDefault(os => os.Id == ordemServico.Id);

        if (ordemServicoExistente is null)
        {
            return null;
        }

        ordemServicoExistente.Status = ordemServico.Status;
        ordemServicoExistente.StatusAprovacaoOrcamento = ordemServico.StatusAprovacaoOrcamento;
        ordemServicoExistente.MockEnvioOrcamento = ordemServico.MockEnvioOrcamento;
        ordemServicoExistente.MotivoRecusaOrcamento = ordemServico.MotivoRecusaOrcamento;
        ordemServicoExistente.DiagnosticoEm = ordemServico.DiagnosticoEm;
        ordemServicoExistente.OrcamentoEnviadoEm = ordemServico.OrcamentoEnviadoEm;
        ordemServicoExistente.ExecucaoIniciadaEm = ordemServico.ExecucaoIniciadaEm;
        ordemServicoExistente.FinalizadaEm = ordemServico.FinalizadaEm;
        ordemServicoExistente.EntregueEm = ordemServico.EntregueEm;

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

using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.InterfaceAdapters.DataSources;

namespace OficinaMecanica.Api.InterfaceAdapters.Gateways;

public class PecaInsumoGateway : IPecaInsumoGateway
{
    private readonly IPecaInsumoDataSource _pecaInsumoDataSource;

    public PecaInsumoGateway(IPecaInsumoDataSource pecaInsumoDataSource)
    {
        _pecaInsumoDataSource = pecaInsumoDataSource;
    }

    public List<PecaInsumo> ObterTodos()
    {
        return _pecaInsumoDataSource.ObterTodos();
    }

    public PecaInsumo? ObterPorId(Guid id)
    {
        return _pecaInsumoDataSource.ObterPorId(id);
    }

    public bool ExistePorNome(string nome)
    {
        return _pecaInsumoDataSource.ExistePorNome(nome);
    }

    public bool ExistePorNomeExcetoId(string nome, Guid id)
    {
        return _pecaInsumoDataSource.ExistePorNomeExcetoId(nome, id);
    }

    public PecaInsumo Adicionar(PecaInsumo pecaInsumo)
    {
        return _pecaInsumoDataSource.Adicionar(pecaInsumo);
    }

    public PecaInsumo? Atualizar(PecaInsumo pecaInsumo)
    {
        return _pecaInsumoDataSource.Atualizar(pecaInsumo);
    }

    public bool Remover(Guid id)
    {
        return _pecaInsumoDataSource.Remover(id);
    }
}

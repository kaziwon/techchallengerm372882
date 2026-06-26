using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.InterfaceAdapters.DataSources;

namespace OficinaMecanica.Api.InterfaceAdapters.Gateways;

public class OrdemServicoGateway : IOrdemServicoGateway
{
    private readonly IOrdemServicoDataSource _ordemServicoDataSource;

    public OrdemServicoGateway(IOrdemServicoDataSource ordemServicoDataSource)
    {
        _ordemServicoDataSource = ordemServicoDataSource;
    }

    public List<OrdemServico> ObterTodas()
    {
        return _ordemServicoDataSource.ObterTodas();
    }

    public List<OrdemServico> ObterPorCpfCnpjCliente(string cpfCnpj)
    {
        return _ordemServicoDataSource.ObterPorCpfCnpjCliente(cpfCnpj);
    }

    public OrdemServico? ObterPorId(Guid id)
    {
        return _ordemServicoDataSource.ObterPorId(id);
    }

    public OrdemServico Adicionar(OrdemServico ordemServico)
    {
        return _ordemServicoDataSource.Adicionar(ordemServico);
    }

    public OrdemServico? Atualizar(OrdemServico ordemServico)
    {
        return _ordemServicoDataSource.Atualizar(ordemServico);
    }
}

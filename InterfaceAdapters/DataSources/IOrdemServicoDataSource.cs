using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.InterfaceAdapters.DataSources;

public interface IOrdemServicoDataSource
{
    List<OrdemServico> ObterTodas();
    List<OrdemServico> ObterPorCpfCnpjCliente(string cpfCnpj);
    OrdemServico? ObterPorId(Guid id);
    OrdemServico Adicionar(OrdemServico ordemServico);
    OrdemServico? Atualizar(OrdemServico ordemServico);
}

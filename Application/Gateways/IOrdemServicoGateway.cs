using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.Gateways;

public interface IOrdemServicoGateway
{
    List<OrdemServico> ObterTodas();
    List<OrdemServico> ObterPorCpfCnpjCliente(string cpfCnpj);
    OrdemServico? ObterPorId(Guid id);
    OrdemServico Adicionar(OrdemServico ordemServico);
    OrdemServico? Atualizar(OrdemServico ordemServico);
}

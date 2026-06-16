using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.Gateways;

public interface IClienteGateway
{
    List<Cliente> ObterTodos();
    Cliente? ObterPorId(Guid id);
    Cliente? ObterPorCpfCnpj(string cpfCnpj);
    bool ExistePorCpfCnpj(string cpfCnpj);
    bool ExistePorCpfCnpjExcetoId(string cpfCnpj, Guid id);
    Cliente Adicionar(Cliente cliente);
    Cliente? Atualizar(Cliente cliente);
    bool Remover(Guid id);
}

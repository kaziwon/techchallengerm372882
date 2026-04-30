using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Domain.Repositories;

public interface IClienteRepository
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

using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Domain.Repositories;

public interface IVeiculoRepository
{
    List<Veiculo> ObterTodos();
    Veiculo? ObterPorId(Guid id);
    bool ExistePorPlaca(string placa);
    bool ExistePorPlacaExcetoId(string placa, Guid id);
    bool ClienteExiste(Guid clienteId);
    Veiculo Adicionar(Veiculo veiculo);
    Veiculo? Atualizar(Veiculo veiculo);
    bool Remover(Guid id);
}

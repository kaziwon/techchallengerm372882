using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.InterfaceAdapters.DataSources;

public interface IVeiculoDataSource
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

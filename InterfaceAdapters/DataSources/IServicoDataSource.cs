using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.InterfaceAdapters.DataSources;

public interface IServicoDataSource
{
    List<Servico> ObterTodos();
    Servico? ObterPorId(Guid id);
    bool ExistePorNome(string nome);
    bool ExistePorNomeExcetoId(string nome, Guid id);
    Servico Adicionar(Servico servico);
    Servico? Atualizar(Servico servico);
    bool Remover(Guid id);
}

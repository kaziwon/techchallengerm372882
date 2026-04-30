using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Domain.Repositories;

public interface IPecaInsumoRepository
{
    List<PecaInsumo> ObterTodos();
    PecaInsumo? ObterPorId(Guid id);
    bool ExistePorNome(string nome);
    bool ExistePorNomeExcetoId(string nome, Guid id);
    PecaInsumo Adicionar(PecaInsumo pecaInsumo);
    PecaInsumo? Atualizar(PecaInsumo pecaInsumo);
    bool Remover(Guid id);
}

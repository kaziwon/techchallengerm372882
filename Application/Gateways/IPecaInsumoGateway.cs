using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.Gateways;

public interface IPecaInsumoGateway
{
    List<PecaInsumo> ObterTodos();
    PecaInsumo? ObterPorId(Guid id);
    bool ExistePorNome(string nome);
    bool ExistePorNomeExcetoId(string nome, Guid id);
    PecaInsumo Adicionar(PecaInsumo pecaInsumo);
    PecaInsumo? Atualizar(PecaInsumo pecaInsumo);
    bool Remover(Guid id);
}

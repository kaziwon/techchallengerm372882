using OficinaMecanica.Api.Application.DTOs;

namespace OficinaMecanica.Api.Application.Interfaces;

public interface IPecaInsumoService
{
    List<PecaInsumoResponseDto> ObterTodos();
    PecaInsumoResponseDto? ObterPorId(Guid id);
    PecaInsumoResponseDto Adicionar(PecaInsumoRequestDto pecaInsumoRequestDto);
    PecaInsumoResponseDto? Atualizar(Guid id, PecaInsumoRequestDto pecaInsumoRequestDto);
    bool Remover(Guid id);
}

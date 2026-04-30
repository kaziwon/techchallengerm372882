using OficinaMecanica.Api.Application.DTOs;

namespace OficinaMecanica.Api.Application.Interfaces;

public interface IServicoService
{
    List<ServicoResponseDto> ObterTodos();
    ServicoResponseDto? ObterPorId(Guid id);
    ServicoResponseDto Adicionar(ServicoRequestDto servicoRequestDto);
    ServicoResponseDto? Atualizar(Guid id, ServicoRequestDto servicoRequestDto);
    bool Remover(Guid id);
}

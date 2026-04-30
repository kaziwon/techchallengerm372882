using OficinaMecanica.Api.Application.DTOs;

namespace OficinaMecanica.Api.Application.Interfaces;

public interface IVeiculoService
{
    List<VeiculoResponseDto> ObterTodos();
    VeiculoResponseDto? ObterPorId(Guid id);
    VeiculoResponseDto Adicionar(VeiculoRequestDto veiculoRequestDto);
    VeiculoResponseDto? Atualizar(Guid id, VeiculoRequestDto veiculoRequestDto);
    bool Remover(Guid id);
}

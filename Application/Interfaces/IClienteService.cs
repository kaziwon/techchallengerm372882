using OficinaMecanica.Api.Application.DTOs;

namespace OficinaMecanica.Api.Application.Interfaces;

public interface IClienteService
{
    List<ClienteResponseDto> ObterTodos();
    ClienteResponseDto? ObterPorId(Guid id);
    ClienteResponseDto Adicionar(ClienteRequestDto clienteRequestDto);
    ClienteResponseDto? Atualizar(Guid id, ClienteRequestDto clienteRequestDto);
    bool Remover(Guid id);
}

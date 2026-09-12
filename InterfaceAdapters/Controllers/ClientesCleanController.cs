using OficinaMecanica.Api.InterfaceAdapters.DTOs;
using OficinaMecanica.Api.Application.UseCases.Clientes;

namespace OficinaMecanica.Api.InterfaceAdapters.Controllers;

public class ClientesCleanController
{
    private readonly ObterTodosClientesUseCase _obterTodosClientesUseCase;
    private readonly ObterClientePorIdUseCase _obterClientePorIdUseCase;
    private readonly CriarClienteUseCase _criarClienteUseCase;
    private readonly AtualizarClienteUseCase _atualizarClienteUseCase;
    private readonly RemoverClienteUseCase _removerClienteUseCase;

    public ClientesCleanController(
        ObterTodosClientesUseCase obterTodosClientesUseCase,
        ObterClientePorIdUseCase obterClientePorIdUseCase,
        CriarClienteUseCase criarClienteUseCase,
        AtualizarClienteUseCase atualizarClienteUseCase,
        RemoverClienteUseCase removerClienteUseCase)
    {
        _obterTodosClientesUseCase = obterTodosClientesUseCase;
        _obterClientePorIdUseCase = obterClientePorIdUseCase;
        _criarClienteUseCase = criarClienteUseCase;
        _atualizarClienteUseCase = atualizarClienteUseCase;
        _removerClienteUseCase = removerClienteUseCase;
    }

    public List<ClienteResponseDto> ObterTodos()
    {
        return _obterTodosClientesUseCase
            .Executar()
            .Select(MapearResponse)
            .ToList();
    }

    public ClienteResponseDto? ObterPorId(Guid id)
    {
        var cliente = _obterClientePorIdUseCase.Executar(id);

        return cliente is null ? null : MapearResponse(cliente);
    }

    public ClienteResponseDto Criar(ClienteRequestDto clienteRequestDto)
    {
        var input = new CriarClienteInput(
            clienteRequestDto.Nome,
            clienteRequestDto.CpfCnpj,
            clienteRequestDto.Email,
            clienteRequestDto.Telefone);

        return MapearResponse(_criarClienteUseCase.Executar(input));
    }

    public ClienteResponseDto? Atualizar(Guid id, ClienteRequestDto clienteRequestDto)
    {
        var input = new AtualizarClienteInput(
            id,
            clienteRequestDto.Nome,
            clienteRequestDto.CpfCnpj,
            clienteRequestDto.Email,
            clienteRequestDto.Telefone);
        var clienteAtualizado = _atualizarClienteUseCase.Executar(input);

        return clienteAtualizado is null ? null : MapearResponse(clienteAtualizado);
    }

    public bool Remover(Guid id)
    {
        return _removerClienteUseCase.Executar(id);
    }

    private static ClienteResponseDto MapearResponse(ClienteOutput cliente)
    {
        return new ClienteResponseDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            CpfCnpj = cliente.CpfCnpj,
            Email = cliente.Email,
            Telefone = cliente.Telefone,
            Ativo = cliente.Ativo
        };
    }
}

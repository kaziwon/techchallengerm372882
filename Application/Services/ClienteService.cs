using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.Interfaces;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.Domain.Repositories;

namespace OficinaMecanica.Api.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;

    public ClienteService(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public List<ClienteResponseDto> ObterTodos()
    {
        var clientes = _clienteRepository.ObterTodos();

        return clientes.Select(cliente => new ClienteResponseDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            CpfCnpj = cliente.CpfCnpj,
            Email = cliente.Email,
            Telefone = cliente.Telefone
        }).ToList();
    }

    public ClienteResponseDto? ObterPorId(Guid id)
    {
        var cliente = _clienteRepository.ObterPorId(id);

        if (cliente is null)
        {
            return null;
        }

        return new ClienteResponseDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            CpfCnpj = cliente.CpfCnpj,
            Email = cliente.Email,
            Telefone = cliente.Telefone
        };
    }

    public ClienteResponseDto Adicionar(ClienteRequestDto clienteRequestDto)
    {
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = clienteRequestDto.Nome,
            CpfCnpj = clienteRequestDto.CpfCnpj,
            Email = clienteRequestDto.Email,
            Telefone = clienteRequestDto.Telefone
        };

        var clienteAdicionado = _clienteRepository.Adicionar(cliente);

        return new ClienteResponseDto
        {
            Id = clienteAdicionado.Id,
            Nome = clienteAdicionado.Nome,
            CpfCnpj = clienteAdicionado.CpfCnpj,
            Email = clienteAdicionado.Email,
            Telefone = clienteAdicionado.Telefone
        };
    }

    public ClienteResponseDto? Atualizar(Guid id, ClienteRequestDto clienteRequestDto)
    {
        var cliente = new Cliente
        {
            Id = id,
            Nome = clienteRequestDto.Nome,
            CpfCnpj = clienteRequestDto.CpfCnpj,
            Email = clienteRequestDto.Email,
            Telefone = clienteRequestDto.Telefone
        };

        var clienteAtualizado = _clienteRepository.Atualizar(cliente);

        if (clienteAtualizado is null)
        {
            return null;
        }

        return new ClienteResponseDto
        {
            Id = clienteAtualizado.Id,
            Nome = clienteAtualizado.Nome,
            CpfCnpj = clienteAtualizado.CpfCnpj,
            Email = clienteAtualizado.Email,
            Telefone = clienteAtualizado.Telefone
        };
    }

    public bool Remover(Guid id)
    {
        return _clienteRepository.Remover(id);
    }
}

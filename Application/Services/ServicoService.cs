using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.Interfaces;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.Domain.Repositories;

namespace OficinaMecanica.Api.Application.Services;

public class ServicoService : IServicoService
{
    private const string NomeJaCadastrado = "Ja existe um serviço cadastrado com este nome.";
    private readonly IServicoRepository _servicoRepository;

    public ServicoService(IServicoRepository servicoRepository)
    {
        _servicoRepository = servicoRepository;
    }

    public List<ServicoResponseDto> ObterTodos()
    {
        return _servicoRepository.ObterTodos().Select(MapearParaResponse).ToList();
    }

    public ServicoResponseDto? ObterPorId(Guid id)
    {
        var servico = _servicoRepository.ObterPorId(id);

        return servico is null ? null : MapearParaResponse(servico);
    }

    public ServicoResponseDto Adicionar(ServicoRequestDto servicoRequestDto)
    {
        if (_servicoRepository.ExistePorNome(servicoRequestDto.Nome))
        {
            throw new InvalidOperationException(NomeJaCadastrado);
        }

        var servico = new Servico
        {
            Id = Guid.NewGuid(),
            Nome = servicoRequestDto.Nome,
            Descricao = servicoRequestDto.Descricao,
            Preco = servicoRequestDto.Preco
        };

        return MapearParaResponse(_servicoRepository.Adicionar(servico));
    }

    public ServicoResponseDto? Atualizar(Guid id, ServicoRequestDto servicoRequestDto)
    {
        if (_servicoRepository.ExistePorNomeExcetoId(servicoRequestDto.Nome, id))
        {
            throw new InvalidOperationException(NomeJaCadastrado);
        }

        var servico = new Servico
        {
            Id = id,
            Nome = servicoRequestDto.Nome,
            Descricao = servicoRequestDto.Descricao,
            Preco = servicoRequestDto.Preco
        };

        var servicoAtualizado = _servicoRepository.Atualizar(servico);

        return servicoAtualizado is null ? null : MapearParaResponse(servicoAtualizado);
    }

    public bool Remover(Guid id)
    {
        return _servicoRepository.Remover(id);
    }

    private static ServicoResponseDto MapearParaResponse(Servico servico)
    {
        return new ServicoResponseDto
        {
            Id = servico.Id,
            Nome = servico.Nome,
            Descricao = servico.Descricao,
            Preco = servico.Preco
        };
    }
}

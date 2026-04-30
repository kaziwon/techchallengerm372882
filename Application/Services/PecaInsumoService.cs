using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.Interfaces;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.Domain.Repositories;

namespace OficinaMecanica.Api.Application.Services;

public class PecaInsumoService : IPecaInsumoService
{
    private const string NomeJaCadastrado = "Ja existe uma peca ou insumo cadastrado com este nome.";
    private readonly IPecaInsumoRepository _pecaInsumoRepository;

    public PecaInsumoService(IPecaInsumoRepository pecaInsumoRepository)
    {
        _pecaInsumoRepository = pecaInsumoRepository;
    }

    public List<PecaInsumoResponseDto> ObterTodos()
    {
        return _pecaInsumoRepository.ObterTodos().Select(MapearParaResponse).ToList();
    }

    public PecaInsumoResponseDto? ObterPorId(Guid id)
    {
        var pecaInsumo = _pecaInsumoRepository.ObterPorId(id);

        return pecaInsumo is null ? null : MapearParaResponse(pecaInsumo);
    }

    public PecaInsumoResponseDto Adicionar(PecaInsumoRequestDto pecaInsumoRequestDto)
    {
        if (_pecaInsumoRepository.ExistePorNome(pecaInsumoRequestDto.Nome))
        {
            throw new InvalidOperationException(NomeJaCadastrado);
        }

        var pecaInsumo = new PecaInsumo
        {
            Id = Guid.NewGuid(),
            Nome = pecaInsumoRequestDto.Nome,
            Descricao = pecaInsumoRequestDto.Descricao,
            PrecoUnitario = pecaInsumoRequestDto.PrecoUnitario,
            QuantidadeEstoque = pecaInsumoRequestDto.QuantidadeEstoque
        };

        return MapearParaResponse(_pecaInsumoRepository.Adicionar(pecaInsumo));
    }

    public PecaInsumoResponseDto? Atualizar(Guid id, PecaInsumoRequestDto pecaInsumoRequestDto)
    {
        if (_pecaInsumoRepository.ExistePorNomeExcetoId(pecaInsumoRequestDto.Nome, id))
        {
            throw new InvalidOperationException(NomeJaCadastrado);
        }

        var pecaInsumo = new PecaInsumo
        {
            Id = id,
            Nome = pecaInsumoRequestDto.Nome,
            Descricao = pecaInsumoRequestDto.Descricao,
            PrecoUnitario = pecaInsumoRequestDto.PrecoUnitario,
            QuantidadeEstoque = pecaInsumoRequestDto.QuantidadeEstoque
        };

        var pecaInsumoAtualizada = _pecaInsumoRepository.Atualizar(pecaInsumo);

        return pecaInsumoAtualizada is null ? null : MapearParaResponse(pecaInsumoAtualizada);
    }

    public bool Remover(Guid id)
    {
        return _pecaInsumoRepository.Remover(id);
    }

    private static PecaInsumoResponseDto MapearParaResponse(PecaInsumo pecaInsumo)
    {
        return new PecaInsumoResponseDto
        {
            Id = pecaInsumo.Id,
            Nome = pecaInsumo.Nome,
            Descricao = pecaInsumo.Descricao,
            PrecoUnitario = pecaInsumo.PrecoUnitario,
            QuantidadeEstoque = pecaInsumo.QuantidadeEstoque
        };
    }
}

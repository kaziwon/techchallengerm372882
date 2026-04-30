using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.Interfaces;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.Domain.Repositories;

namespace OficinaMecanica.Api.Application.Services;

public class VeiculoService : IVeiculoService
{
    private const string PlacaJaCadastrada = "Ja existe um veiculo cadastrado com esta placa.";
    private const string ClienteNaoEncontrado = "O cliente informado nao foi encontrado.";
    private readonly IVeiculoRepository _veiculoRepository;

    public VeiculoService(IVeiculoRepository veiculoRepository)
    {
        _veiculoRepository = veiculoRepository;
    }

    public List<VeiculoResponseDto> ObterTodos()
    {
        return _veiculoRepository.ObterTodos().Select(MapearParaResponse).ToList();
    }

    public VeiculoResponseDto? ObterPorId(Guid id)
    {
        var veiculo = _veiculoRepository.ObterPorId(id);

        return veiculo is null ? null : MapearParaResponse(veiculo);
    }

    public VeiculoResponseDto Adicionar(VeiculoRequestDto veiculoRequestDto)
    {
        if (!_veiculoRepository.ClienteExiste(veiculoRequestDto.ClienteId))
        {
            throw new InvalidOperationException(ClienteNaoEncontrado);
        }

        if (_veiculoRepository.ExistePorPlaca(veiculoRequestDto.Placa))
        {
            throw new InvalidOperationException(PlacaJaCadastrada);
        }

        var veiculo = new Veiculo
        {
            Id = Guid.NewGuid(),
            ClienteId = veiculoRequestDto.ClienteId,
            Placa = NormalizarPlaca(veiculoRequestDto.Placa),
            Marca = veiculoRequestDto.Marca,
            Modelo = veiculoRequestDto.Modelo,
            Ano = veiculoRequestDto.Ano
        };

        return MapearParaResponse(_veiculoRepository.Adicionar(veiculo));
    }

    public VeiculoResponseDto? Atualizar(Guid id, VeiculoRequestDto veiculoRequestDto)
    {
        if (!_veiculoRepository.ClienteExiste(veiculoRequestDto.ClienteId))
        {
            throw new InvalidOperationException(ClienteNaoEncontrado);
        }

        if (_veiculoRepository.ExistePorPlacaExcetoId(veiculoRequestDto.Placa, id))
        {
            throw new InvalidOperationException(PlacaJaCadastrada);
        }

        var veiculo = new Veiculo
        {
            Id = id,
            ClienteId = veiculoRequestDto.ClienteId,
            Placa = NormalizarPlaca(veiculoRequestDto.Placa),
            Marca = veiculoRequestDto.Marca,
            Modelo = veiculoRequestDto.Modelo,
            Ano = veiculoRequestDto.Ano
        };

        var veiculoAtualizado = _veiculoRepository.Atualizar(veiculo);

        return veiculoAtualizado is null ? null : MapearParaResponse(veiculoAtualizado);
    }

    public bool Remover(Guid id)
    {
        return _veiculoRepository.Remover(id);
    }

    private static VeiculoResponseDto MapearParaResponse(Veiculo veiculo)
    {
        return new VeiculoResponseDto
        {
            Id = veiculo.Id,
            ClienteId = veiculo.ClienteId,
            Placa = veiculo.Placa,
            Marca = veiculo.Marca,
            Modelo = veiculo.Modelo,
            Ano = veiculo.Ano
        };
    }

    private static string NormalizarPlaca(string placa)
    {
        return placa.Trim().ToUpperInvariant().Replace("-", "");
    }
}

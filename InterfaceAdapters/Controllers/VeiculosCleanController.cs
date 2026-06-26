using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.UseCases.Veiculos;

namespace OficinaMecanica.Api.InterfaceAdapters.Controllers;

public class VeiculosCleanController
{
    private readonly ObterTodosVeiculosUseCase _obterTodosVeiculosUseCase;
    private readonly ObterVeiculoPorIdUseCase _obterVeiculoPorIdUseCase;
    private readonly CriarVeiculoUseCase _criarVeiculoUseCase;
    private readonly AtualizarVeiculoUseCase _atualizarVeiculoUseCase;
    private readonly RemoverVeiculoUseCase _removerVeiculoUseCase;

    public VeiculosCleanController(
        ObterTodosVeiculosUseCase obterTodosVeiculosUseCase,
        ObterVeiculoPorIdUseCase obterVeiculoPorIdUseCase,
        CriarVeiculoUseCase criarVeiculoUseCase,
        AtualizarVeiculoUseCase atualizarVeiculoUseCase,
        RemoverVeiculoUseCase removerVeiculoUseCase)
    {
        _obterTodosVeiculosUseCase = obterTodosVeiculosUseCase;
        _obterVeiculoPorIdUseCase = obterVeiculoPorIdUseCase;
        _criarVeiculoUseCase = criarVeiculoUseCase;
        _atualizarVeiculoUseCase = atualizarVeiculoUseCase;
        _removerVeiculoUseCase = removerVeiculoUseCase;
    }

    public List<VeiculoResponseDto> ObterTodos()
    {
        return _obterTodosVeiculosUseCase.Executar().Select(MapearResponse).ToList();
    }

    public VeiculoResponseDto? ObterPorId(Guid id)
    {
        var veiculo = _obterVeiculoPorIdUseCase.Executar(id);

        return veiculo is null ? null : MapearResponse(veiculo);
    }

    public VeiculoResponseDto Criar(VeiculoRequestDto veiculoRequestDto)
    {
        var input = new VeiculoInput(
            veiculoRequestDto.ClienteId,
            veiculoRequestDto.Placa,
            veiculoRequestDto.Marca,
            veiculoRequestDto.Modelo,
            veiculoRequestDto.Ano);

        return MapearResponse(_criarVeiculoUseCase.Executar(input));
    }

    public VeiculoResponseDto? Atualizar(Guid id, VeiculoRequestDto veiculoRequestDto)
    {
        var input = new VeiculoInput(
            veiculoRequestDto.ClienteId,
            veiculoRequestDto.Placa,
            veiculoRequestDto.Marca,
            veiculoRequestDto.Modelo,
            veiculoRequestDto.Ano);
        var veiculo = _atualizarVeiculoUseCase.Executar(id, input);

        return veiculo is null ? null : MapearResponse(veiculo);
    }

    public bool Remover(Guid id)
    {
        return _removerVeiculoUseCase.Executar(id);
    }

    private static VeiculoResponseDto MapearResponse(VeiculoOutput veiculo)
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
}

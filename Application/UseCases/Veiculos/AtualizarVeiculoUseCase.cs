using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.Veiculos;

public class AtualizarVeiculoUseCase
{
    private const string PlacaJaCadastrada = "Ja existe um veiculo cadastrado com esta placa.";
    private const string ClienteNaoEncontrado = "O cliente informado nao foi encontrado.";
    private readonly IVeiculoGateway _veiculoGateway;

    public AtualizarVeiculoUseCase(IVeiculoGateway veiculoGateway)
    {
        _veiculoGateway = veiculoGateway;
    }

    public VeiculoOutput? Executar(Guid id, VeiculoInput input)
    {
        if (!_veiculoGateway.ClienteExiste(input.ClienteId))
        {
            throw new InvalidOperationException(ClienteNaoEncontrado);
        }

        if (_veiculoGateway.ExistePorPlacaExcetoId(input.Placa, id))
        {
            throw new InvalidOperationException(PlacaJaCadastrada);
        }

        var veiculo = new Veiculo
        {
            Id = id,
            ClienteId = input.ClienteId,
            Placa = NormalizarPlaca(input.Placa),
            Marca = input.Marca,
            Modelo = input.Modelo,
            Ano = input.Ano
        };

        var veiculoAtualizado = _veiculoGateway.Atualizar(veiculo);

        return veiculoAtualizado is null ? null : VeiculoOutputMapper.Mapear(veiculoAtualizado);
    }

    private static string NormalizarPlaca(string placa)
    {
        return placa.Trim().ToUpperInvariant().Replace("-", "");
    }
}

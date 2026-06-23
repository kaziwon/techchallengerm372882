using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.Veiculos;

public class CriarVeiculoUseCase
{
    private const string PlacaJaCadastrada = "Ja existe um veiculo cadastrado com esta placa.";
    private const string ClienteNaoEncontrado = "O cliente informado nao foi encontrado.";
    private readonly IVeiculoGateway _veiculoGateway;

    public CriarVeiculoUseCase(IVeiculoGateway veiculoGateway)
    {
        _veiculoGateway = veiculoGateway;
    }

    public VeiculoOutput Executar(VeiculoInput input)
    {
        ValidarClienteEPlaca(input.ClienteId, input.Placa);

        var veiculo = new Veiculo
        {
            Id = Guid.NewGuid(),
            ClienteId = input.ClienteId,
            Placa = NormalizarPlaca(input.Placa),
            Marca = input.Marca,
            Modelo = input.Modelo,
            Ano = input.Ano
        };

        return VeiculoOutputMapper.Mapear(_veiculoGateway.Adicionar(veiculo));
    }

    private void ValidarClienteEPlaca(Guid clienteId, string placa)
    {
        if (!_veiculoGateway.ClienteExiste(clienteId))
        {
            throw new InvalidOperationException(ClienteNaoEncontrado);
        }

        if (_veiculoGateway.ExistePorPlaca(placa))
        {
            throw new InvalidOperationException(PlacaJaCadastrada);
        }
    }

    private static string NormalizarPlaca(string placa)
    {
        return placa.Trim().ToUpperInvariant().Replace("-", "");
    }
}

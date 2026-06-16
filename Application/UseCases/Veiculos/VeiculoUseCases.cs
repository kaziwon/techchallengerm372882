using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.Veiculos;

public record VeiculoInput(Guid ClienteId, string Placa, string Marca, string Modelo, int Ano);

public class VeiculoOutput
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Ano { get; set; }
}

public class ObterTodosVeiculosUseCase
{
    private readonly IVeiculoGateway _veiculoGateway;

    public ObterTodosVeiculosUseCase(IVeiculoGateway veiculoGateway)
    {
        _veiculoGateway = veiculoGateway;
    }

    public List<VeiculoOutput> Executar()
    {
        return _veiculoGateway.ObterTodos().Select(VeiculoOutputMapper.Mapear).ToList();
    }
}

public class ObterVeiculoPorIdUseCase
{
    private readonly IVeiculoGateway _veiculoGateway;

    public ObterVeiculoPorIdUseCase(IVeiculoGateway veiculoGateway)
    {
        _veiculoGateway = veiculoGateway;
    }

    public VeiculoOutput? Executar(Guid id)
    {
        var veiculo = _veiculoGateway.ObterPorId(id);

        return veiculo is null ? null : VeiculoOutputMapper.Mapear(veiculo);
    }
}

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

public class RemoverVeiculoUseCase
{
    private readonly IVeiculoGateway _veiculoGateway;

    public RemoverVeiculoUseCase(IVeiculoGateway veiculoGateway)
    {
        _veiculoGateway = veiculoGateway;
    }

    public bool Executar(Guid id)
    {
        return _veiculoGateway.Remover(id);
    }
}

internal static class VeiculoOutputMapper
{
    public static VeiculoOutput Mapear(Veiculo veiculo)
    {
        return new VeiculoOutput
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

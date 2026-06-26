using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Application.Exceptions;
using OficinaMecanica.Api.Application.UseCases.Veiculos;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.UnitTests;

public class VeiculoUseCasesTests
{
    [Fact]
    public void Criar_DeveLancarExcecao_QuandoClienteNaoExistir()
    {
        var gateway = new FakeVeiculoGateway();
        var useCase = new CriarVeiculoUseCase(gateway, new FakePlacaVeiculoValidatorGateway());

        Assert.Throws<InvalidOperationException>(() => useCase.Executar(CriarInput()));
    }

    [Fact]
    public void Criar_DeveLancarExcecao_QuandoPlacaJaExistir()
    {
        var gateway = new FakeVeiculoGateway
        {
            ClienteExisteResult = true,
            ExistePorPlacaResult = true
        };
        var useCase = new CriarVeiculoUseCase(gateway, new FakePlacaVeiculoValidatorGateway());

        Assert.Throws<InvalidOperationException>(() => useCase.Executar(CriarInput()));
    }

    [Fact]
    public void Criar_DeveNormalizarPlacaERetornarOutput()
    {
        var gateway = new FakeVeiculoGateway
        {
            ClienteExisteResult = true
        };
        var useCase = new CriarVeiculoUseCase(gateway, new FakePlacaVeiculoValidatorGateway());

        var output = useCase.Executar(CriarInput("abc-1d23"));

        Assert.Equal("ABC1D23", output.Placa);
        Assert.Single(gateway.Veiculos);
    }

    [Fact]
    public void Atualizar_DeveRetornarNull_QuandoVeiculoNaoExistir()
    {
        var gateway = new FakeVeiculoGateway
        {
            ClienteExisteResult = true
        };
        var useCase = new AtualizarVeiculoUseCase(gateway, new FakePlacaVeiculoValidatorGateway());

        var output = useCase.Executar(Guid.NewGuid(), CriarInput());

        Assert.Null(output);
    }

    private static VeiculoInput CriarInput(string placa = "BRA2E19")
    {
        return new VeiculoInput(Guid.NewGuid(), placa, "Toyota", "Corolla", 2022);
    }

    [Fact]
    public void Criar_DeveLancarValidacaoException_QuandoPlacaForInvalida()
    {
        var gateway = new FakeVeiculoGateway
        {
            ClienteExisteResult = true
        };
        var placaValidator = new FakePlacaVeiculoValidatorGateway
        {
            EhValidaResult = false
        };
        var useCase = new CriarVeiculoUseCase(gateway, placaValidator);

        Assert.Throws<ValidacaoException>(() => useCase.Executar(CriarInput()));
    }

    private sealed class FakeVeiculoGateway : IVeiculoGateway
    {
        public List<Veiculo> Veiculos { get; } = [];
        public bool ClienteExisteResult { get; set; }
        public bool ExistePorPlacaResult { get; set; }
        public bool ExistePorPlacaExcetoIdResult { get; set; }

        public Veiculo Adicionar(Veiculo veiculo)
        {
            Veiculos.Add(veiculo);
            return veiculo;
        }

        public Veiculo? Atualizar(Veiculo veiculo)
        {
            var existente = Veiculos.FirstOrDefault(v => v.Id == veiculo.Id);

            if (existente is null)
            {
                return null;
            }

            existente.ClienteId = veiculo.ClienteId;
            existente.Placa = veiculo.Placa;
            existente.Marca = veiculo.Marca;
            existente.Modelo = veiculo.Modelo;
            existente.Ano = veiculo.Ano;
            return existente;
        }

        public bool ClienteExiste(Guid clienteId)
        {
            return ClienteExisteResult;
        }

        public bool ExistePorPlaca(string placa)
        {
            return ExistePorPlacaResult || Veiculos.Any(veiculo => veiculo.Placa == placa);
        }

        public bool ExistePorPlacaExcetoId(string placa, Guid id)
        {
            return ExistePorPlacaExcetoIdResult || Veiculos.Any(veiculo => veiculo.Placa == placa && veiculo.Id != id);
        }

        public Veiculo? ObterPorId(Guid id)
        {
            return Veiculos.FirstOrDefault(veiculo => veiculo.Id == id);
        }

        public List<Veiculo> ObterTodos()
        {
            return Veiculos;
        }

        public bool Remover(Guid id)
        {
            return true;
        }
    }
}

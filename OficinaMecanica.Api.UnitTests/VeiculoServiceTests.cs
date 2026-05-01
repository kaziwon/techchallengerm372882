using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.Services;
using OficinaMecanica.Api.Domain.Entities;
using OficinaMecanica.Api.Domain.Repositories;

namespace OficinaMecanica.Api.UnitTests;

public class VeiculoServiceTests
{
    [Fact]
    public void Adicionar_DeveLancarExcecao_QuandoClienteNaoExistir()
    {
        var repository = new FakeVeiculoRepository();
        var service = new VeiculoService(repository);

        Assert.Throws<InvalidOperationException>(() => service.Adicionar(CriarRequest()));
    }

    [Fact]
    public void Adicionar_DeveLancarExcecao_QuandoPlacaJaExistir()
    {
        var repository = new FakeVeiculoRepository
        {
            ClienteExisteResult = true,
            ExistePorPlacaResult = true
        };
        var service = new VeiculoService(repository);

        Assert.Throws<InvalidOperationException>(() => service.Adicionar(CriarRequest()));
    }

    [Fact]
    public void Adicionar_DeveNormalizarPlacaERetornarResponse()
    {
        var repository = new FakeVeiculoRepository
        {
            ClienteExisteResult = true
        };
        var service = new VeiculoService(repository);

        var response = service.Adicionar(CriarRequest("abc-1d23"));

        Assert.Equal("ABC1D23", response.Placa);
        Assert.Single(repository.Veiculos);
    }

    [Fact]
    public void Atualizar_DeveRetornarNull_QuandoVeiculoNaoExistir()
    {
        var repository = new FakeVeiculoRepository
        {
            ClienteExisteResult = true
        };
        var service = new VeiculoService(repository);

        var response = service.Atualizar(Guid.NewGuid(), CriarRequest());

        Assert.Null(response);
    }

    private static VeiculoRequestDto CriarRequest(string placa = "BRA2E19")
    {
        return new VeiculoRequestDto
        {
            ClienteId = Guid.NewGuid(),
            Placa = placa,
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2022
        };
    }

    private sealed class FakeVeiculoRepository : IVeiculoRepository
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

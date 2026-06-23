using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class CriarOrdemServicoUseCase
{
    private const string ClienteNaoEncontrado = "Cliente nao encontrado para o CPF/CNPJ informado.";
    private const string VeiculoNaoEncontrado = "Veiculo nao encontrado.";
    private const string VeiculoNaoPertenceAoCliente = "O veiculo informado nao pertence ao cliente.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;
    private readonly IClienteGateway _clienteGateway;
    private readonly IVeiculoGateway _veiculoGateway;

    private readonly IServicoGateway _servicoGateway;

    private readonly IPecaInsumoGateway _pecaInsumoGateway;

    public CriarOrdemServicoUseCase(
        IOrdemServicoGateway ordemServicoGateway,
        IClienteGateway clienteGateway,
        IVeiculoGateway veiculoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
        _clienteGateway = clienteGateway;
        _veiculoGateway = veiculoGateway;
    }

    public OrdemServicoOutput Executar(CriarOrdemServicoInput input)
    {
        var cliente = null as Cliente;
        
        if(input.CpfCnpj is not null)
        {
            cliente = _clienteGateway.ObterPorCpfCnpj(NormalizarCpfCnpj(input.CpfCnpj));

        }else if(input.Cliente is not null)
        {
            var clienteExistente = _clienteGateway.ObterPorCpfCnpj(NormalizarCpfCnpj(input.Cliente.CpfCnpj));
            if(clienteExistente is not null)
            {
                cliente = clienteExistente;
            }
            else
            {
                cliente = _clienteGateway.Adicionar(new Cliente
                {
                    Id = Guid.NewGuid(),
                    Nome = input.Cliente.Nome,
                    CpfCnpj = NormalizarCpfCnpj(input.Cliente.CpfCnpj),
                    Telefone = input.Cliente.Telefone,
                    Email = input.Cliente.Email, 
                });
            }
      
        }

        if (cliente is null)
        {
            throw new InvalidOperationException(ClienteNaoEncontrado);
        }

        var veiculo = null as Veiculo;

        if(input.VeiculoId is not null)
        {
            veiculo = _veiculoGateway.ObterPorId(input.VeiculoId.Value);
        }else if(input.Veiculo is not null)
        {
            veiculo = _veiculoGateway.Adicionar(new Veiculo
            {
                Id = Guid.NewGuid(),
                ClienteId = cliente.Id,
                Placa = input.Veiculo.Placa,
                Modelo = input.Veiculo.Modelo,
                Ano = input.Veiculo.Ano,
                Marca = input.Veiculo.Marca
            });
        }

        if (veiculo is null)
        {
            throw new InvalidOperationException(VeiculoNaoEncontrado);
        }

        if (veiculo.ClienteId != cliente.Id)
        {
            throw new InvalidOperationException(VeiculoNaoPertenceAoCliente);
        }



        var agora = DateTime.UtcNow;
        var ordemServico = new OrdemServico
        {
            Id = Guid.NewGuid(),
            ClienteId = cliente.Id,
            VeiculoId = veiculo.Id,
            Status = StatusOrdemServico.Recebida,
            StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Pendente,
            ValorTotalServicos = 0,
            ValorTotalPecasInsumos = 0,
            ValorTotalOrcamento = 0,
            EnvioOrcamento = string.Empty,
            CriadaEm = agora
        };

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Adicionar(ordemServico));
    }

    private static string NormalizarCpfCnpj(string cpfCnpj)
    {
        return new string(cpfCnpj.Where(char.IsDigit).ToArray());
    }
}

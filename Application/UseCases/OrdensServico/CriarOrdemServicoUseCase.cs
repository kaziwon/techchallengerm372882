using OficinaMecanica.Api.Application.Exceptions;
using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class CriarOrdemServicoUseCase
{
    private const string DadosCriacaoInvalidos = "Informe CpfCnpj e VeiculoId para usar cadastro existente, ou Cliente e Veiculo para criar cadastro completo.";
    private const string CpfCnpjInvalido = "O CPF/CNPJ informado é inválido.";
    private const string PlacaInvalida = "A placa informada é inválida.";
    private const string ClienteNaoEncontrado = "Cliente nao encontrado para o CPF/CNPJ informado.";
    private const string VeiculoNaoEncontrado = "Veiculo nao encontrado.";
    private const string VeiculoNaoPertenceAoCliente = "O veiculo informado nao pertence ao cliente.";
    private const string CpfCnpjJaCadastrado = "Ja existe um cliente cadastrado com este CPF/CNPJ.";
    private const string PlacaJaCadastrada = "Ja existe um veiculo cadastrado com esta placa.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;
    private readonly IClienteGateway _clienteGateway;
    private readonly IVeiculoGateway _veiculoGateway;
    private readonly IServicoGateway _servicoGateway;
    private readonly IPecaInsumoGateway _pecaInsumoGateway;
    private readonly ICpfCnpjValidatorGateway _cpfCnpjValidatorGateway;
    private readonly IPlacaVeiculoValidatorGateway _placaVeiculoValidatorGateway;

    public CriarOrdemServicoUseCase(
        IOrdemServicoGateway ordemServicoGateway,
        IClienteGateway clienteGateway,
        IVeiculoGateway veiculoGateway,
        IServicoGateway servicoGateway,
        IPecaInsumoGateway pecaInsumoGateway,
        ICpfCnpjValidatorGateway cpfCnpjValidatorGateway,
        IPlacaVeiculoValidatorGateway placaVeiculoValidatorGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
        _clienteGateway = clienteGateway;
        _veiculoGateway = veiculoGateway;
        _servicoGateway = servicoGateway;
        _pecaInsumoGateway = pecaInsumoGateway;
        _cpfCnpjValidatorGateway = cpfCnpjValidatorGateway;
        _placaVeiculoValidatorGateway = placaVeiculoValidatorGateway;
    }

    public OrdemServicoOutput Executar(CriarOrdemServicoInput input)
    {
        var (cliente, veiculo) = ObterClienteEVeiculo(input);

        var agora = DateTime.UtcNow;
        var ordemServicoId = Guid.NewGuid();
        var itens = OrdemServicoItensBuilder.Montar(
            ordemServicoId,
            input.ServicoIds,
            input.PecasInsumos,
            _servicoGateway,
            _pecaInsumoGateway);
        var ordemServico = new OrdemServico
        {
            Id = ordemServicoId,
            ClienteId = cliente.Id,
            VeiculoId = veiculo.Id,
            Status = StatusOrdemServico.Recebida,
            StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Pendente,
            ValorTotalServicos = itens.ValorTotalServicos,
            ValorTotalPecasInsumos = itens.ValorTotalPecasInsumos,
            ValorTotalOrcamento = itens.ValorTotalOrcamento,
            EnvioOrcamento = string.Empty,
            CriadaEm = agora,
            ItensServico = itens.ItensServico,
            ItensPecaInsumo = itens.ItensPecaInsumo
        };

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Adicionar(ordemServico));
    }

    private (Cliente Cliente, Veiculo Veiculo) ObterClienteEVeiculo(CriarOrdemServicoInput input)
    {
        if (DeveUsarCadastroExistente(input))
        {
            return ObterClienteEVeiculoExistentes(input);
        }

        if (DeveCriarCadastroCompleto(input))
        {
            return CriarClienteEVeiculo(input);
        }

        throw new ValidacaoException(DadosCriacaoInvalidos);
    }

    private (Cliente Cliente, Veiculo Veiculo) ObterClienteEVeiculoExistentes(CriarOrdemServicoInput input)
    {
        var cpfCnpj = ValidarCpfCnpj(input.CpfCnpj!);
        var cliente = _clienteGateway.ObterPorCpfCnpj(cpfCnpj);

        if (cliente is null)
        {
            throw new InvalidOperationException(ClienteNaoEncontrado);
        }

        var veiculo = _veiculoGateway.ObterPorId(input.VeiculoId!.Value);

        if (veiculo is null)
        {
            throw new InvalidOperationException(VeiculoNaoEncontrado);
        }

        if (veiculo.ClienteId != cliente.Id)
        {
            throw new InvalidOperationException(VeiculoNaoPertenceAoCliente);
        }

        return (cliente, veiculo);
    }

    private (Cliente Cliente, Veiculo Veiculo) CriarClienteEVeiculo(CriarOrdemServicoInput input)
    {
        var clienteInput = input.Cliente!;
        var veiculoInput = input.Veiculo!;
        var cpfCnpj = ValidarCpfCnpj(clienteInput.CpfCnpj);
        var placa = ValidarPlaca(veiculoInput.Placa);

        if (_clienteGateway.ExistePorCpfCnpj(cpfCnpj))
        {
            throw new InvalidOperationException(CpfCnpjJaCadastrado);
        }

        if (_veiculoGateway.ExistePorPlaca(placa))
        {
            throw new InvalidOperationException(PlacaJaCadastrada);
        }

        var cliente = _clienteGateway.Adicionar(new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = clienteInput.Nome,
            CpfCnpj = cpfCnpj,
            Telefone = clienteInput.Telefone,
            Email = clienteInput.Email
        });

        var veiculo = _veiculoGateway.Adicionar(new Veiculo
        {
            Id = Guid.NewGuid(),
            ClienteId = cliente.Id,
            Placa = placa,
            Modelo = veiculoInput.Modelo,
            Ano = veiculoInput.Ano,
            Marca = veiculoInput.Marca
        });

        return (cliente, veiculo);
    }

    private static bool DeveUsarCadastroExistente(CriarOrdemServicoInput input)
    {
        return !string.IsNullOrWhiteSpace(input.CpfCnpj)
            && input.VeiculoId.HasValue
            && input.Cliente is null
            && input.Veiculo is null;
    }

    private static bool DeveCriarCadastroCompleto(CriarOrdemServicoInput input)
    {
        return string.IsNullOrWhiteSpace(input.CpfCnpj)
            && !input.VeiculoId.HasValue
            && input.Cliente is not null
            && input.Veiculo is not null;
    }

    private string ValidarCpfCnpj(string cpfCnpj)
    {
        if (!_cpfCnpjValidatorGateway.EhValido(cpfCnpj))
        {
            throw new ValidacaoException(CpfCnpjInvalido);
        }

        return _cpfCnpjValidatorGateway.Normalizar(cpfCnpj);
    }

    private string ValidarPlaca(string placa)
    {
        if (!_placaVeiculoValidatorGateway.EhValida(placa))
        {
            throw new ValidacaoException(PlacaInvalida);
        }

        return _placaVeiculoValidatorGateway.Normalizar(placa);
    }
}

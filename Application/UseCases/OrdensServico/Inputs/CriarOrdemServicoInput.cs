namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;
using OficinaMecanica.Api.Application.UseCases.Veiculos;
using OficinaMecanica.Api.Application.UseCases.Clientes;
using OficinaMecanica.Api.Application.UseCases.Servicos;
using OficinaMecanica.Api.Application.UseCases.PecasInsumos;
public class CriarOrdemServicoInput
{
    public CriarOrdemServicoInput(string cpfCnpj, Guid? veiculoId, VeiculoInput veiculo, CriarClienteInput cliente)
    {
        CpfCnpj = cpfCnpj;
        VeiculoId = veiculoId;
        Veiculo = veiculo;
        Cliente = cliente;
     
    }

    public string CpfCnpj { get; }
    public Guid? VeiculoId { get; }

    public VeiculoInput Veiculo {get;}
    public CriarClienteInput Cliente {get;}

}

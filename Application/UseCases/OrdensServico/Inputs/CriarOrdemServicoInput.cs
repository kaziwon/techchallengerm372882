namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class CriarOrdemServicoInput
{
    public CriarOrdemServicoInput(string cpfCnpj, Guid veiculoId)
        : this(cpfCnpj, veiculoId, null, null, [], [])
    {
    }

    public CriarOrdemServicoInput(
        string? cpfCnpj,
        Guid? veiculoId,
        CriarOrdemServicoClienteInput? cliente,
        CriarOrdemServicoVeiculoInput? veiculo)
        : this(cpfCnpj, veiculoId, cliente, veiculo, [], [])
    {
    }

    public CriarOrdemServicoInput(
        string? cpfCnpj,
        Guid? veiculoId,
        CriarOrdemServicoClienteInput? cliente,
        CriarOrdemServicoVeiculoInput? veiculo,
        List<Guid> servicoIds,
        List<OrdemServicoItemPecaInsumoInput> pecasInsumos)
    {
        CpfCnpj = cpfCnpj;
        VeiculoId = veiculoId;
        Cliente = cliente;
        Veiculo = veiculo;
        ServicoIds = servicoIds;
        PecasInsumos = pecasInsumos;
    }

    public string? CpfCnpj { get; }
    public Guid? VeiculoId { get; }
    public CriarOrdemServicoClienteInput? Cliente { get; }
    public CriarOrdemServicoVeiculoInput? Veiculo { get; }
    public List<Guid> ServicoIds { get; }
    public List<OrdemServicoItemPecaInsumoInput> PecasInsumos { get; }
}

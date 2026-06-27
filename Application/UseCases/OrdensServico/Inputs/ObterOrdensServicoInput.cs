using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class ObterOrdensServicoInput
{
    public ObterOrdensServicoInput(
        StatusOrdemServico? status = null,
        OrdemServicoOrdenacaoData ordenacaoData = OrdemServicoOrdenacaoData.MaisAntigasPrimeiro)
    {
        Status = status;
        OrdenacaoData = ordenacaoData;
    }

    public StatusOrdemServico? Status { get; }
    public OrdemServicoOrdenacaoData OrdenacaoData { get; }
}

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class OrdemServicoOrcamentoInput
{
    public OrdemServicoOrcamentoInput(List<Guid> servicoIds, List<OrdemServicoItemPecaInsumoInput> pecasInsumos)
    {
        ServicoIds = servicoIds;
        PecasInsumos = pecasInsumos;
    }

    public List<Guid> ServicoIds { get; }
    public List<OrdemServicoItemPecaInsumoInput> PecasInsumos { get; }
}

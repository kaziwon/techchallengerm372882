namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class OrdemServicoItemPecaInsumoInput
{
    public OrdemServicoItemPecaInsumoInput(Guid pecaInsumoId, int quantidade)
    {
        PecaInsumoId = pecaInsumoId;
        Quantidade = quantidade;
    }

    public Guid PecaInsumoId { get; }
    public int Quantidade { get; }
}

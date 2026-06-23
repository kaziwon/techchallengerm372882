namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class RecusarOrcamentoInput
{
    public RecusarOrcamentoInput(string motivoRecusa)
    {
        MotivoRecusa = motivoRecusa;
    }

    public string MotivoRecusa { get; }
}

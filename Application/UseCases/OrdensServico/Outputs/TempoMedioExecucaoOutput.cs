namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class TempoMedioExecucaoOutput
{
    public int QuantidadeOrdensConsideradas { get; set; }
    public double TempoMedioExecucaoEmMinutos { get; set; }
    public string TempoMedioExecucaoFormatado { get; set; } = string.Empty;
}

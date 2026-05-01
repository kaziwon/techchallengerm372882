namespace OficinaMecanica.Api.Application.DTOs;

public class TempoMedioExecucaoResponseDto
{
    public int QuantidadeOrdensConsideradas { get; set; }
    public double TempoMedioExecucaoEmMinutos { get; set; }
    public string TempoMedioExecucaoFormatado { get; set; } = string.Empty;
}

using OficinaMecanica.Api.Application.Gateways;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class ObterTempoMedioExecucaoUseCase
{
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public ObterTempoMedioExecucaoUseCase(IOrdemServicoGateway ordemServicoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
    }

    public TempoMedioExecucaoOutput Executar()
    {
        var ordensFinalizadas = _ordemServicoGateway.ObterTodas()
            .Where(ordemServico => ordemServico.ExecucaoIniciadaEm.HasValue && ordemServico.FinalizadaEm.HasValue)
            .ToList();

        if (ordensFinalizadas.Count == 0)
        {
            return new TempoMedioExecucaoOutput
            {
                QuantidadeOrdensConsideradas = 0,
                TempoMedioExecucaoEmMinutos = 0,
                TempoMedioExecucaoFormatado = "00:00:00"
            };
        }

        var media = TimeSpan.FromTicks((long)ordensFinalizadas
            .Average(ordemServico => (ordemServico.FinalizadaEm!.Value - ordemServico.ExecucaoIniciadaEm!.Value).Ticks));

        return new TempoMedioExecucaoOutput
        {
            QuantidadeOrdensConsideradas = ordensFinalizadas.Count,
            TempoMedioExecucaoEmMinutos = Math.Round(media.TotalMinutes, 2),
            TempoMedioExecucaoFormatado = media.ToString(@"hh\:mm\:ss")
        };
    }
}

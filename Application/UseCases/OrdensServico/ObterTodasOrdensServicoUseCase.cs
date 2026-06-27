using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class ObterTodasOrdensServicoUseCase
{
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public ObterTodasOrdensServicoUseCase(IOrdemServicoGateway ordemServicoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
    }

    public List<OrdemServicoOutput> Executar(ObterOrdensServicoInput? input = null)
    {
        input ??= new ObterOrdensServicoInput();

        var ordens = _ordemServicoGateway
            .ObterTodas()
            .Where(DeveAparecerNaListagem);

        if (input.Status.HasValue)
        {
            ordens = ordens.Where(ordemServico => ordemServico.Status == input.Status.Value);
        }

        ordens = input.OrdenacaoData == OrdemServicoOrdenacaoData.MaisNovasPrimeiro
            ? ordens
                .OrderBy(PrioridadeStatus)
                .ThenByDescending(ordemServico => ordemServico.CriadaEm)
            : ordens
                .OrderBy(PrioridadeStatus)
                .ThenBy(ordemServico => ordemServico.CriadaEm);

        return ordens.Select(OrdemServicoOutputMapper.Mapear).ToList();
    }

    private static bool DeveAparecerNaListagem(OrdemServico ordemServico)
    {
        return ordemServico.Status is not StatusOrdemServico.Finalizada
            and not StatusOrdemServico.Entregue
            and not StatusOrdemServico.Cancelada;
    }

    private static int PrioridadeStatus(OrdemServico ordemServico)
    {
        return ordemServico.Status switch
        {
            StatusOrdemServico.EmExecucao => 1,
            StatusOrdemServico.AguardandoAprovacao => 2,
            StatusOrdemServico.EmDiagnostico => 3,
            StatusOrdemServico.Recebida => 4,
            _ => 99
        };
    }
}

using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.OrdensServico;

public class RecusarOrcamentoUseCase
{
    private const string OrcamentoNaoEstaAguardandoAprovacao = "A ordem de servico nao esta aguardando aprovacao do orcamento.";
    private readonly IOrdemServicoGateway _ordemServicoGateway;

    public RecusarOrcamentoUseCase(IOrdemServicoGateway ordemServicoGateway)
    {
        _ordemServicoGateway = ordemServicoGateway;
    }

    public OrdemServicoOutput? Executar(Guid id, RecusarOrcamentoInput input)
    {
        var ordemServico = _ordemServicoGateway.ObterPorId(id);

        if (ordemServico is null)
        {
            return null;
        }

        if (ordemServico.Status != StatusOrdemServico.AguardandoAprovacao)
        {
            throw new InvalidOperationException(OrcamentoNaoEstaAguardandoAprovacao);
        }

        ordemServico.Status = StatusOrdemServico.EmDiagnostico;
        ordemServico.StatusAprovacaoOrcamento = StatusAprovacaoOrcamento.Recusado;
        ordemServico.MotivoRecusaOrcamento = input.MotivoRecusa;

        return OrdemServicoOutputMapper.Mapear(_ordemServicoGateway.Atualizar(ordemServico)!);
    }
}

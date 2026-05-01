using OficinaMecanica.Api.Application.DTOs;

namespace OficinaMecanica.Api.Application.Interfaces;

public interface IOrdemServicoService
{
    List<OrdemServicoResponseDto> ObterTodas();
    TempoMedioExecucaoResponseDto ObterTempoMedioExecucao();
    List<OrdemServicoResponseDto> ObterPorCpfCnpjCliente(string cpfCnpj);
    OrdemServicoResponseDto? ObterPorId(Guid id);
    OrdemServicoResponseDto Criar(OrdemServicoRequestDto ordemServicoRequestDto);
    OrdemServicoResponseDto? IniciarDiagnostico(Guid id);
    OrdemServicoResponseDto? EnviarOrcamento(Guid id, OrdemServicoOrcamentoRequestDto requestDto);
    OrdemServicoResponseDto? AprovarOrcamento(Guid id);
    OrdemServicoResponseDto? RecusarOrcamento(Guid id, OrdemServicoRespostaAprovacaoRequestDto requestDto);
    OrdemServicoResponseDto? Cancelar(Guid id);
    OrdemServicoResponseDto? Finalizar(Guid id);
    OrdemServicoResponseDto? Entregar(Guid id);
}

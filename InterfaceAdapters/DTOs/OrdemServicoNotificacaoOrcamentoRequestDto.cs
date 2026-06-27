namespace OficinaMecanica.Api.InterfaceAdapters.DTOs;

public class OrdemServicoNotificacaoOrcamentoRequestDto
{
    public bool Aprovado { get; set; }
    public string MotivoRecusa { get; set; } = string.Empty;
}

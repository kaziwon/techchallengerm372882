namespace OficinaMecanica.Api.InterfaceAdapters.DTOs;

public class OrdemServicoRequestDto
{
    public string? CpfCnpj { get; set; }
    public Guid? VeiculoId { get; set; }
    public OrdemServicoClienteRequestDto? Cliente { get; set; }
    public OrdemServicoVeiculoRequestDto? Veiculo { get; set; }
    public List<Guid> ServicoIds { get; set; } = [];
    public List<OrdemServicoItemPecaInsumoRequestDto> PecasInsumos { get; set; } = [];
}

public class OrdemServicoClienteRequestDto
{
    public string Nome { get; set; } = string.Empty;
    public string CpfCnpj { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
}

public class OrdemServicoVeiculoRequestDto
{
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Ano { get; set; }
}

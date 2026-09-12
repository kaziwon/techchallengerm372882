namespace OficinaMecanica.Api.Application.UseCases.Clientes;

public class ClienteOutput
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CpfCnpj { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}

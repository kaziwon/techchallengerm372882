namespace OficinaMecanica.Api.Application.UseCases.Servicos;

public class ServicoOutput
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
}

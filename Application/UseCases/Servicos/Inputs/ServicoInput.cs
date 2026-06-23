namespace OficinaMecanica.Api.Application.UseCases.Servicos;

public class ServicoInput
{
    public ServicoInput(string nome, string descricao, decimal preco)
    {
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
    }

    public string Nome { get; }
    public string Descricao { get; }
    public decimal Preco { get; }
}

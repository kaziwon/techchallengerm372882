namespace OficinaMecanica.Api.Application.UseCases.PecasInsumos;

public class PecaInsumoInput
{
    public PecaInsumoInput(string nome, string descricao, decimal precoUnitario, int quantidadeEstoque)
    {
        Nome = nome;
        Descricao = descricao;
        PrecoUnitario = precoUnitario;
        QuantidadeEstoque = quantidadeEstoque;
    }

    public string Nome { get; }
    public string Descricao { get; }
    public decimal PrecoUnitario { get; }
    public int QuantidadeEstoque { get; }
}

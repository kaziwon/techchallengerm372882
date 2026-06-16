namespace OficinaMecanica.Api.Application.UseCases.Clientes;

public class CriarClienteInput
{
    public CriarClienteInput(string nome, string cpfCnpj, string email, string telefone)
    {
        Nome = nome;
        CpfCnpj = cpfCnpj;
        Email = email;
        Telefone = telefone;
    }

    public string Nome { get; }
    public string CpfCnpj { get; }
    public string Email { get; }
    public string Telefone { get; }
}

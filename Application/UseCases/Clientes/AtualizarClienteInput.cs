namespace OficinaMecanica.Api.Application.UseCases.Clientes;

public class AtualizarClienteInput
{
    public AtualizarClienteInput(Guid id, string nome, string cpfCnpj, string email, string telefone)
    {
        Id = id;
        Nome = nome;
        CpfCnpj = cpfCnpj;
        Email = email;
        Telefone = telefone;
    }

    public Guid Id { get; }
    public string Nome { get; }
    public string CpfCnpj { get; }
    public string Email { get; }
    public string Telefone { get; }
}

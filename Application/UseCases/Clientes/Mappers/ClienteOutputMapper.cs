using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.Clientes;

internal static class ClienteOutputMapper
{
    public static ClienteOutput Mapear(Cliente cliente)
    {
        return new ClienteOutput
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            CpfCnpj = cliente.CpfCnpj,
            Email = cliente.Email,
            Telefone = cliente.Telefone
        };
    }
}

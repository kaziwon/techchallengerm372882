using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.Servicos;

internal static class ServicoOutputMapper
{
    public static ServicoOutput Mapear(Servico servico)
    {
        return new ServicoOutput
        {
            Id = servico.Id,
            Nome = servico.Nome,
            Descricao = servico.Descricao,
            Preco = servico.Preco
        };
    }
}

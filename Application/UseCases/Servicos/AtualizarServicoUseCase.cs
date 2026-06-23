using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.Servicos;

public class AtualizarServicoUseCase
{
    private const string NomeJaCadastrado = "Ja existe um serviço cadastrado com este nome.";
    private readonly IServicoGateway _servicoGateway;

    public AtualizarServicoUseCase(IServicoGateway servicoGateway)
    {
        _servicoGateway = servicoGateway;
    }

    public ServicoOutput? Executar(Guid id, ServicoInput input)
    {
        if (_servicoGateway.ExistePorNomeExcetoId(input.Nome, id))
        {
            throw new InvalidOperationException(NomeJaCadastrado);
        }

        var servico = new Servico
        {
            Id = id,
            Nome = input.Nome,
            Descricao = input.Descricao,
            Preco = input.Preco
        };

        var servicoAtualizado = _servicoGateway.Atualizar(servico);

        return servicoAtualizado is null ? null : ServicoOutputMapper.Mapear(servicoAtualizado);
    }
}

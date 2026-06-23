using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.Servicos;

public class CriarServicoUseCase
{
    private const string NomeJaCadastrado = "Ja existe um serviço cadastrado com este nome.";
    private readonly IServicoGateway _servicoGateway;

    public CriarServicoUseCase(IServicoGateway servicoGateway)
    {
        _servicoGateway = servicoGateway;
    }

    public ServicoOutput Executar(ServicoInput input)
    {
        if (_servicoGateway.ExistePorNome(input.Nome))
        {
            throw new InvalidOperationException(NomeJaCadastrado);
        }

        var servico = new Servico
        {
            Id = Guid.NewGuid(),
            Nome = input.Nome,
            Descricao = input.Descricao,
            Preco = input.Preco
        };

        return ServicoOutputMapper.Mapear(_servicoGateway.Adicionar(servico));
    }
}

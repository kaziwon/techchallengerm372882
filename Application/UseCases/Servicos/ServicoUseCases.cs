using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Application.UseCases.Servicos;

public record ServicoInput(string Nome, string Descricao, decimal Preco);

public class ServicoOutput
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
}

public class ObterTodosServicosUseCase
{
    private readonly IServicoGateway _servicoGateway;

    public ObterTodosServicosUseCase(IServicoGateway servicoGateway)
    {
        _servicoGateway = servicoGateway;
    }

    public List<ServicoOutput> Executar()
    {
        return _servicoGateway.ObterTodos().Select(ServicoOutputMapper.Mapear).ToList();
    }
}

public class ObterServicoPorIdUseCase
{
    private readonly IServicoGateway _servicoGateway;

    public ObterServicoPorIdUseCase(IServicoGateway servicoGateway)
    {
        _servicoGateway = servicoGateway;
    }

    public ServicoOutput? Executar(Guid id)
    {
        var servico = _servicoGateway.ObterPorId(id);

        return servico is null ? null : ServicoOutputMapper.Mapear(servico);
    }
}

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

public class RemoverServicoUseCase
{
    private readonly IServicoGateway _servicoGateway;

    public RemoverServicoUseCase(IServicoGateway servicoGateway)
    {
        _servicoGateway = servicoGateway;
    }

    public bool Executar(Guid id)
    {
        return _servicoGateway.Remover(id);
    }
}

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

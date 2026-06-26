using OficinaMecanica.Api.Application.DTOs;
using OficinaMecanica.Api.Application.UseCases.Servicos;

namespace OficinaMecanica.Api.InterfaceAdapters.Controllers;

public class ServicosCleanController
{
    private readonly ObterTodosServicosUseCase _obterTodosServicosUseCase;
    private readonly ObterServicoPorIdUseCase _obterServicoPorIdUseCase;
    private readonly CriarServicoUseCase _criarServicoUseCase;
    private readonly AtualizarServicoUseCase _atualizarServicoUseCase;
    private readonly RemoverServicoUseCase _removerServicoUseCase;

    public ServicosCleanController(
        ObterTodosServicosUseCase obterTodosServicosUseCase,
        ObterServicoPorIdUseCase obterServicoPorIdUseCase,
        CriarServicoUseCase criarServicoUseCase,
        AtualizarServicoUseCase atualizarServicoUseCase,
        RemoverServicoUseCase removerServicoUseCase)
    {
        _obterTodosServicosUseCase = obterTodosServicosUseCase;
        _obterServicoPorIdUseCase = obterServicoPorIdUseCase;
        _criarServicoUseCase = criarServicoUseCase;
        _atualizarServicoUseCase = atualizarServicoUseCase;
        _removerServicoUseCase = removerServicoUseCase;
    }

    public List<ServicoResponseDto> ObterTodos()
    {
        return _obterTodosServicosUseCase.Executar().Select(MapearResponse).ToList();
    }

    public ServicoResponseDto? ObterPorId(Guid id)
    {
        var servico = _obterServicoPorIdUseCase.Executar(id);

        return servico is null ? null : MapearResponse(servico);
    }

    public ServicoResponseDto Criar(ServicoRequestDto servicoRequestDto)
    {
        var input = new ServicoInput(
            servicoRequestDto.Nome,
            servicoRequestDto.Descricao,
            servicoRequestDto.Preco);

        return MapearResponse(_criarServicoUseCase.Executar(input));
    }

    public ServicoResponseDto? Atualizar(Guid id, ServicoRequestDto servicoRequestDto)
    {
        var input = new ServicoInput(
            servicoRequestDto.Nome,
            servicoRequestDto.Descricao,
            servicoRequestDto.Preco);
        var servico = _atualizarServicoUseCase.Executar(id, input);

        return servico is null ? null : MapearResponse(servico);
    }

    public bool Remover(Guid id)
    {
        return _removerServicoUseCase.Executar(id);
    }

    private static ServicoResponseDto MapearResponse(ServicoOutput servico)
    {
        return new ServicoResponseDto
        {
            Id = servico.Id,
            Nome = servico.Nome,
            Descricao = servico.Descricao,
            Preco = servico.Preco
        };
    }
}

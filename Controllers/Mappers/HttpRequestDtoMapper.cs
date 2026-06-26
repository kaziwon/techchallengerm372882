using CleanDtos = OficinaMecanica.Api.InterfaceAdapters.DTOs;
using HttpDtos = OficinaMecanica.Api.Controllers.DTOs;

namespace OficinaMecanica.Api.Controllers.Mappers;

public static class HttpRequestDtoMapper
{
    public static CleanDtos.LoginRequestDto ParaCleanDto(this HttpDtos.LoginRequestDto dto)
    {
        return new CleanDtos.LoginRequestDto
        {
            Username = dto.Username,
            Password = dto.Password
        };
    }

    public static CleanDtos.ClienteRequestDto ParaCleanDto(this HttpDtos.ClienteRequestDto dto)
    {
        return new CleanDtos.ClienteRequestDto
        {
            Nome = dto.Nome,
            CpfCnpj = dto.CpfCnpj,
            Email = dto.Email,
            Telefone = dto.Telefone
        };
    }

    public static CleanDtos.VeiculoRequestDto ParaCleanDto(this HttpDtos.VeiculoRequestDto dto)
    {
        return new CleanDtos.VeiculoRequestDto
        {
            ClienteId = dto.ClienteId,
            Placa = dto.Placa,
            Marca = dto.Marca,
            Modelo = dto.Modelo,
            Ano = dto.Ano
        };
    }

    public static CleanDtos.ServicoRequestDto ParaCleanDto(this HttpDtos.ServicoRequestDto dto)
    {
        return new CleanDtos.ServicoRequestDto
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Preco = dto.Preco
        };
    }

    public static CleanDtos.PecaInsumoRequestDto ParaCleanDto(this HttpDtos.PecaInsumoRequestDto dto)
    {
        return new CleanDtos.PecaInsumoRequestDto
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            PrecoUnitario = dto.PrecoUnitario,
            QuantidadeEstoque = dto.QuantidadeEstoque
        };
    }

    public static CleanDtos.OrdemServicoRequestDto ParaCleanDto(this HttpDtos.OrdemServicoRequestDto dto)
    {
        return new CleanDtos.OrdemServicoRequestDto
        {
            CpfCnpj = dto.CpfCnpj,
            VeiculoId = dto.VeiculoId,
            Cliente = dto.Cliente.ParaCleanDto(),
            Veiculo = dto.Veiculo.ParaCleanDto(),
            ServicoIds = dto.ServicoIds ?? [],
            PecasInsumos = dto.PecasInsumos?.Select(ParaCleanDto).ToList() ?? []
        };
    }

    public static CleanDtos.OrdemServicoOrcamentoRequestDto ParaCleanDto(this HttpDtos.OrdemServicoOrcamentoRequestDto dto)
    {
        return new CleanDtos.OrdemServicoOrcamentoRequestDto
        {
            ServicoIds = dto.ServicoIds ?? [],
            PecasInsumos = dto.PecasInsumos?.Select(ParaCleanDto).ToList() ?? []
        };
    }

    public static CleanDtos.OrdemServicoRespostaAprovacaoRequestDto ParaCleanDto(
        this HttpDtos.OrdemServicoRespostaAprovacaoRequestDto dto)
    {
        return new CleanDtos.OrdemServicoRespostaAprovacaoRequestDto
        {
            MotivoRecusa = dto.MotivoRecusa
        };
    }

    private static CleanDtos.OrdemServicoItemPecaInsumoRequestDto ParaCleanDto(
        this HttpDtos.OrdemServicoItemPecaInsumoRequestDto dto)
    {
        return new CleanDtos.OrdemServicoItemPecaInsumoRequestDto
        {
            PecaInsumoId = dto.PecaInsumoId,
            Quantidade = dto.Quantidade
        };
    }

    private static CleanDtos.OrdemServicoClienteRequestDto? ParaCleanDto(
        this HttpDtos.OrdemServicoClienteRequestDto? dto)
    {
        return dto is null
            ? null
            : new CleanDtos.OrdemServicoClienteRequestDto
            {
                Nome = dto.Nome,
                CpfCnpj = dto.CpfCnpj,
                Email = dto.Email,
                Telefone = dto.Telefone
            };
    }

    private static CleanDtos.OrdemServicoVeiculoRequestDto? ParaCleanDto(
        this HttpDtos.OrdemServicoVeiculoRequestDto? dto)
    {
        return dto is null
            ? null
            : new CleanDtos.OrdemServicoVeiculoRequestDto
            {
                Placa = dto.Placa,
                Marca = dto.Marca,
                Modelo = dto.Modelo,
                Ano = dto.Ano
            };
    }
}

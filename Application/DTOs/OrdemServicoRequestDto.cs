using System.ComponentModel.DataAnnotations;
using OficinaMecanica.Api.Application.Validators;
using OficinaMecanica.Api.Application.UseCases.Veiculos;
using OficinaMecanica.Api.Application.UseCases.Clientes;
using OficinaMecanica.Api.Application.UseCases.Servicos;
using OficinaMecanica.Api.Application.UseCases.PecasInsumos;
namespace OficinaMecanica.Api.Application.DTOs;

public class OrdemServicoRequestDto
{
    [CpfCnpj]
    public string CpfCnpj { get; set; } = string.Empty;

    public Guid? VeiculoId { get; set; }

    public VeiculoInput? Veiculo {get; set;}
    public CriarClienteInput? Cliente {get; set;}
    public List<Guid> ServicoIds { get; set; } = [];
    public List<OrdemServicoItemPecaInsumoRequestDto> PecasInsumos { get; set; } = [];
}

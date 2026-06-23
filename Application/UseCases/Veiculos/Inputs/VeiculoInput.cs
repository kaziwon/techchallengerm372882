namespace OficinaMecanica.Api.Application.UseCases.Veiculos;

public class VeiculoInput
{
    public VeiculoInput(Guid clienteId, string placa, string marca, string modelo, int ano)
    {
        ClienteId = clienteId;
        Placa = placa;
        Marca = marca;
        Modelo = modelo;
        Ano = ano;
    }

    public Guid ClienteId { get; }
    public string Placa { get; }
    public string Marca { get; }
    public string Modelo { get; }
    public int Ano { get; }
}

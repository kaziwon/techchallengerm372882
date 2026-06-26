namespace OficinaMecanica.Api.InterfaceAdapters.DataSources;

public interface IPlacaVeiculoValidatorSource
{
    bool EhValida(string? placa);
    string Normalizar(string placa);
}

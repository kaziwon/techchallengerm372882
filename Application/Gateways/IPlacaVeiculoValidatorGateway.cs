namespace OficinaMecanica.Api.Application.Gateways;

public interface IPlacaVeiculoValidatorGateway
{
    bool EhValida(string? placa);
    string Normalizar(string placa);
}

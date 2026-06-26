namespace OficinaMecanica.Api.InterfaceAdapters.DataSources;

public interface ICpfCnpjValidatorSource
{
    bool EhValido(string? cpfCnpj);
    string Normalizar(string cpfCnpj);
}

using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.InterfaceAdapters.DataSources;

namespace OficinaMecanica.Api.InterfaceAdapters.Gateways;

public class CpfCnpjValidatorGateway : ICpfCnpjValidatorGateway
{
    private readonly ICpfCnpjValidatorSource _cpfCnpjValidatorSource;

    public CpfCnpjValidatorGateway(ICpfCnpjValidatorSource cpfCnpjValidatorSource)
    {
        _cpfCnpjValidatorSource = cpfCnpjValidatorSource;
    }

    public bool EhValido(string? cpfCnpj)
    {
        return _cpfCnpjValidatorSource.EhValido(cpfCnpj);
    }

    public string Normalizar(string cpfCnpj)
    {
        return _cpfCnpjValidatorSource.Normalizar(cpfCnpj);
    }
}

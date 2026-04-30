using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace OficinaMecanica.Api.Application.Validators;

public class CpfCnpjAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string cpfCnpj || string.IsNullOrWhiteSpace(cpfCnpj))
        {
            return new ValidationResult("O campo CPF/CNPJ é obrigatório.");
        }

        var apenasNumeros = Regex.Replace(cpfCnpj, "[^0-9]", "");

        if (apenasNumeros.Length == 11 && EhCpfValido(apenasNumeros))
        {
            return ValidationResult.Success;
        }

        if (apenasNumeros.Length == 14 && EhCnpjValido(apenasNumeros))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult("O CPF/CNPJ informado é inválido.");
    }

    private static bool EhCpfValido(string cpf)
    {
        if (TodosOsDigitosSaoIguais(cpf))
        {
            return false;
        }

        var somaPrimeiroDigito = 0;

        for (var i = 0; i < 9; i++)
        {
            somaPrimeiroDigito += (cpf[i] - '0') * (10 - i);
        }

        var restoPrimeiroDigito = somaPrimeiroDigito % 11;
        var primeiroDigitoVerificador = restoPrimeiroDigito < 2 ? 0 : 11 - restoPrimeiroDigito;

        if (cpf[9] - '0' != primeiroDigitoVerificador)
        {
            return false;
        }

        var somaSegundoDigito = 0;

        for (var i = 0; i < 10; i++)
        {
            somaSegundoDigito += (cpf[i] - '0') * (11 - i);
        }

        var restoSegundoDigito = somaSegundoDigito % 11;
        var segundoDigitoVerificador = restoSegundoDigito < 2 ? 0 : 11 - restoSegundoDigito;

        return cpf[10] - '0' == segundoDigitoVerificador;
    }

    private static bool EhCnpjValido(string cnpj)
    {
        if (TodosOsDigitosSaoIguais(cnpj))
        {
            return false;
        }

        int[] pesosPrimeiroDigito = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] pesosSegundoDigito = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var somaPrimeiroDigito = 0;

        for (var i = 0; i < 12; i++)
        {
            somaPrimeiroDigito += (cnpj[i] - '0') * pesosPrimeiroDigito[i];
        }

        var restoPrimeiroDigito = somaPrimeiroDigito % 11;
        var primeiroDigitoVerificador = restoPrimeiroDigito < 2 ? 0 : 11 - restoPrimeiroDigito;

        if (cnpj[12] - '0' != primeiroDigitoVerificador)
        {
            return false;
        }

        var somaSegundoDigito = 0;

        for (var i = 0; i < 13; i++)
        {
            somaSegundoDigito += (cnpj[i] - '0') * pesosSegundoDigito[i];
        }

        var restoSegundoDigito = somaSegundoDigito % 11;
        var segundoDigitoVerificador = restoSegundoDigito < 2 ? 0 : 11 - restoSegundoDigito;

        return cnpj[13] - '0' == segundoDigitoVerificador;
    }

    private static bool TodosOsDigitosSaoIguais(string valor)
    {
        return valor.All(digito => digito == valor[0]);
    }
}

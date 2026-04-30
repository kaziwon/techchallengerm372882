using System.Text.Json.Serialization;

namespace OficinaMecanica.Api.Domain.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StatusAprovacaoOrcamento
{
    Pendente = 1,
    Aprovado = 2,
    Recusado = 3
}

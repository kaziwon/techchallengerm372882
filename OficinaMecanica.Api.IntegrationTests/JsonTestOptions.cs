using System.Text.Json;
using System.Text.Json.Serialization;

namespace OficinaMecanica.Api.IntegrationTests;

internal static class JsonTestOptions
{
    public static readonly JsonSerializerOptions Value = Criar();

    private static JsonSerializerOptions Criar()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}

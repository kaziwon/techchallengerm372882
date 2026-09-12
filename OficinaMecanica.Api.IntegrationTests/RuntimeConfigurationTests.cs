using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace OficinaMecanica.Api.IntegrationTests;

public class RuntimeConfigurationTests
{
    [Fact]
    public async Task Swagger_DeveSerExpostoEmProducaoQuandoHabilitado()
    {
        await using var factory = new CustomWebApplicationFactory();
        await using var productionFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Swagger:Enabled"] = "true"
                }));
        });
        using var client = productionFactory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

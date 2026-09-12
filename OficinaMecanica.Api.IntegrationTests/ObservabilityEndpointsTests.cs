using System.Net;
using OficinaMecanica.Api.Infrastructure.Observability;

namespace OficinaMecanica.Api.IntegrationTests;

public class ObservabilityEndpointsTests
{
    [Fact]
    public async Task Health_DeveRetornarCorrelationIdGeradoPelaApi()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues(CorrelationIdMiddleware.HeaderName, out var values));
        Assert.False(string.IsNullOrWhiteSpace(values.Single()));
    }

    [Fact]
    public async Task Health_DevePropagarCorrelationIdInformadoPeloCliente()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        const string correlationId = "correlacao-teste-observabilidade";
        request.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(correlationId, response.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Single());
    }

}

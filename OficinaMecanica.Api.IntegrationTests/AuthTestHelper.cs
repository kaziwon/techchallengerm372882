using System.Net.Http.Headers;
using System.Net.Http.Json;
using OficinaMecanica.Api.InterfaceAdapters.DTOs;

namespace OficinaMecanica.Api.IntegrationTests;

internal static class AuthTestHelper
{
    public static async Task AuthenticateAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Username = "admin",
            Password = "Admin@123"
        });

        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>(JsonTestOptions.Value);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse!.Token);
    }
}

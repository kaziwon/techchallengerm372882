using System.Net;
using System.Net.Http.Json;
using OficinaMecanica.Api.Application.DTOs;

namespace OficinaMecanica.Api.IntegrationTests;

public class ClienteEndpointsTests
{
    [Fact]
    public async Task GetClientes_DeveRetornarOk()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);

        var response = await client.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostCliente_DeveCriarClienteERetornarCreated()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);
        var request = CriarRequest();

        var response = await client.PostAsJsonAsync("/api/clientes", request);
        var cliente = await response.Content.ReadFromJsonAsync<ClienteResponseDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(cliente);
        Assert.Equal(request.CpfCnpj, cliente.CpfCnpj);
    }

    [Fact]
    public async Task PostCliente_DeveRetornarBadRequest_QuandoCpfCnpjForInvalido()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);
        var request = new ClienteRequestDto
        {
            Nome = "Joao Silva",
            CpfCnpj = "123",
            Email = "joao@email.com",
            Telefone = "11999999999"
        };

        var response = await client.PostAsJsonAsync("/api/clientes", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostCliente_DeveRetornarConflict_QuandoCpfCnpjForDuplicado()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);
        var request = CriarRequest();

        await client.PostAsJsonAsync("/api/clientes", request);
        var response = await client.PostAsJsonAsync("/api/clientes", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PutCliente_DeveAtualizarCliente_QuandoExistir()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);
        var request = CriarRequest();
        var createResponse = await client.PostAsJsonAsync("/api/clientes", request);
        var clienteCriado = await createResponse.Content.ReadFromJsonAsync<ClienteResponseDto>();
        var updateRequest = new ClienteRequestDto
        {
            Nome = "Joao Silva Atualizado",
            CpfCnpj = request.CpfCnpj,
            Email = "joao.atualizado@email.com",
            Telefone = "11888887777"
        };

        var response = await client.PutAsJsonAsync($"/api/clientes/{clienteCriado!.Id}", updateRequest);
        var clienteAtualizado = await response.Content.ReadFromJsonAsync<ClienteResponseDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(clienteAtualizado);
        Assert.Equal(updateRequest.Nome, clienteAtualizado.Nome);
    }

    [Fact]
    public async Task DeleteCliente_DeveRemoverCliente_QuandoExistir()
    {
        await using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client);
        var request = CriarRequest();
        var createResponse = await client.PostAsJsonAsync("/api/clientes", request);
        var clienteCriado = await createResponse.Content.ReadFromJsonAsync<ClienteResponseDto>();

        var deleteResponse = await client.DeleteAsync($"/api/clientes/{clienteCriado!.Id}");
        var getResponse = await client.GetAsync($"/api/clientes/{clienteCriado.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private static ClienteRequestDto CriarRequest()
    {
        return new ClienteRequestDto
        {
            Nome = "Maria Oliveira",
            CpfCnpj = "11222333000181",
            Email = "maria@email.com",
            Telefone = "11988887777"
        };
    }
}

using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using fiap_5nett_tech.api;
using Microsoft.AspNetCore.Hosting;

namespace fiap_5nett_tech.IntegrationTest;

public class IntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IntegrationTest(WebApplicationFactory<Program> factory)
    {
        // Define o ambiente de teste ao criar o cliente
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing"); // Define o ambiente como "Testing"
        }).CreateClient();
    }

    [Fact]
    public async Task Test_GetContacts_ShouldReturnOk()
    {
        // Faz uma requisição ao endpoint e verifica se retorna 200 (OK)
        var response = await _client.GetAsync("/api/contacts");
        response.EnsureSuccessStatusCode(); // Verifica se o status é 2xx
    }
}
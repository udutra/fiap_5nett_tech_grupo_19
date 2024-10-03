using System.Net;
using System.Net.Http.Json;
using fiap_5nett_tech.Application.DataTransfer.Request;
using fiap_5nett_tech.Application.DataTransfer.Response;
using fiap_5nett_tech.Domain.Entities;
using fiap_5nett_tech.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace fiap_5nett_tech.api.IntegrationTest;
public class ContactControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public ContactControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {                
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                             
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });
                                
                var serviceProvider = services.BuildServiceProvider();
                using (var scope = serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                }
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task CreateContact_ReturnsSuccess()
    {
        //Caminho feliz
        var contactRequest = new ContactRequest
        {
            Name = "Joao da Silva",
            Email = "user@example.com",
            PhoneNumber = "996968512",
            Ddd = 14
        };
        
        var response = await _client.PostAsJsonAsync("/api/Contact", contactRequest);
                
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);       

        // Cleanup: Limpa o banco de dados ao final do teste
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
        }

    }

    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task CreateContact_ReturnsBadRequest()
    {        
        // Arrange - Caso de erro (BadRequest)
        var invalidContactRequest = new ContactRequest
        {
            Name = "", 
            Email = "invalid-email", 
            PhoneNumber = "123", 
            Ddd = 0 
        };

        // Act - Requisição inválida
        var invalidResponse = await _client.PostAsJsonAsync("/api/Contact", invalidContactRequest);

        // Assert - Verifica se retornou BadRequest
        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);

        // Cleanup: Limpa o banco de dados ao final do teste
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
        }
    }


    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task UpdateContact_ReturnsSuccess()
    {
        // Arrange
        var contactRequest = new ContactRequest
        {
            Name = "Joao da Silva",
            Email = "user@example.com",
            PhoneNumber = "996968512",
            Ddd = 14
        };

        // Cria um contato primeiro
        await _client.PostAsJsonAsync("/api/Contact", contactRequest);

        // Atualiza o contato
        var updateRequest = new ContactRequest
        {
            Name = "Atualizado Silva",
            Email = "user@atualizado.com",
            PhoneNumber = "996968512",
            Ddd = 14
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/Contact", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Cleanup: Limpa o banco de dados ao final do teste
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
        }
    }


    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task UpdateContact_ReturnsBadReques()
    {
        // Arrange
        var contactRequest = new ContactRequest
        {
            Name = "Joao ",
            Email = "user@.com",
            PhoneNumber = "9969",
            Ddd = 14
        };      

        // Act
        var response = await _client.PutAsJsonAsync("/api/Contact", contactRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Cleanup: Limpa o banco de dados ao final do teste
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
        }
    }

    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task GetContactByDddAndPhone_ReturnsSuccess()
    {
        // Arrange
        var contactRequest = new ContactRequest
        {
            Name = "Joao da Silva",
            Email = "user@example.com",
            PhoneNumber = "996968512",
            Ddd = 14
        };

        // Cria um contato primeiro
        await _client.PostAsJsonAsync("/api/Contact", contactRequest);

        // Act
        var response = await _client.GetAsync("/api/Contact/14/996968512");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Cleanup: Limpa o banco de dados ao final do teste
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
        }
    }


    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task GetContactByDddAndPhone_ReturnsBadRequest()
    {      
        // Act
        var response = await _client.GetAsync("/api/Contact/14/996968512");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Cleanup: Limpa o banco de dados ao final do teste
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
        }
    }


    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task DeleteContact_ReturnsSuccess()
    {
        // Arrange
        var contactRequest = new ContactRequest
        {
            Name = "Joao da Silva",
            Email = "user@example.com",
            PhoneNumber = "996968512",
            Ddd = 15
        };

        // Cria um contato primeiro
        await _client.PostAsJsonAsync("/api/Contact", contactRequest);

        // Act
        var response = await _client.DeleteAsync("/api/Contact/15/996968512");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Cleanup: Limpa o banco de dados ao final do teste
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
        }
    }


    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task DeleteContact_ReturnsBadRequest()
    {
        // Act
        var response = await _client.DeleteAsync("/api/Contact/15/996968512");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Cleanup: Limpa o banco de dados ao final do teste
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
        }
    }


    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task GetOneContactId_ReturnsSuccess()
    {
        // Arrange
        var contactRequest = new ContactRequest
        {
            Name = "Joao da Silva",
            Email = "user@example.com",
            PhoneNumber = "996968512",
            Ddd = 15
        };

        var responseCreate = await _client.PostAsJsonAsync("/api/Contact", contactRequest);

        var responseGet = await _client.GetAsync("/api/Contact/15/996968512");

        Assert.Equal(HttpStatusCode.OK, responseGet.StatusCode);

        var contact = await responseGet.Content.ReadFromJsonAsync<ContactResponse<Contact>>();

        Assert.NotNull(contact);
        Assert.NotNull(contact.Data);

        var id = contact.Data.Id;

        var responseGetById = await _client.GetAsync($"/api/Contact/{id}");

        Assert.Equal(HttpStatusCode.OK, responseGetById.StatusCode);

        // Cleanup: Limpa o banco de dados ao final do teste
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
        }
    }


    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task GetOneContactId_ReturnsNotFoud()
    {
        //Guid newGuid = Guid.NewGuid();
        var newGuid = "7377d891 - f966 - 48a9 - b7c6 - 1469b5925037";

        var responseGetById = await _client.GetAsync($"/api/Contact/{newGuid}");

        Assert.Equal(HttpStatusCode.NotFound, responseGetById.StatusCode);

        // Cleanup: Limpa o banco de dados ao final do teste
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
        }
    }


    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task GetAllContacts_ReturnsPagedResponseSucess()
    {
        // Arrange
        var contactRequest1 = new ContactRequest
        {
            Name = "Joao da Silva",
            Email = "joao@example.com",
            PhoneNumber = "996968512",
            Ddd = 15
        };

        var contactRequest2 = new ContactRequest
        {
            Name = "Maria Souza",
            Email = "maria@example.com",
            PhoneNumber = "999999999",
            Ddd = 11
        };

        // Cria dois contatos
        await _client.PostAsJsonAsync("/api/Contact", contactRequest1);
        await _client.PostAsJsonAsync("/api/Contact", contactRequest2);

        // Define parâmetros de paginação e filtros
        var getAllContactRequest = new
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var response = await _client.GetAsync($"/api/Contact?PageNumber={getAllContactRequest.PageNumber}&PageSize={getAllContactRequest.PageSize}");

        // Assert
        response.EnsureSuccessStatusCode();

        // Desserializa o conteúdo da resposta
        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedContactResponse<List<Contact>>>();

        // Verifica se a resposta contém contatos
        Assert.NotNull(pagedResponse);
        Assert.NotNull(pagedResponse.Data);
        Assert.True(pagedResponse.Data.Count > 0);

        // Verifica se os contatos foram paginados corretamente
        Assert.Equal(2, pagedResponse.TotalCount); // Deve haver 2 contatos criados
        Assert.Equal(1, pagedResponse.CurrentPage);
        Assert.Equal(10, pagedResponse.PageSize);

        // Cleanup: Limpa o banco de dados ao final do teste
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
        }
    }


    [Fact]
    [Trait("Category", "IntegrationTest")]
    public async Task GetAllContacts_ReturnsPagedResponseBadRequest()
    {
        // Define parâmetros de paginação e filtros
        var getAllContactRequest = new
        {
            PageNumber = 1,
            PageSize = 'x'
        };

        // Act
        var response = await _client.GetAsync($"/api/Contact?PageNumber={getAllContactRequest.PageNumber}&PageSize={getAllContactRequest.PageSize}");        

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Cleanup: Limpa o banco de dados ao final do teste
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
        }
    }


}

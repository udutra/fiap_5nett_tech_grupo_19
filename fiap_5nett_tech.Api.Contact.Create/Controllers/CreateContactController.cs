using System.Text;
using System.Text.Json;
using fiap_5nett_tech.Application.DataTransfer.Request;
using fiap_5nett_tech.Domain.RabbitMqConfiguration;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace fiap_5nett_tech.Api.Contact.Create.Controllers;

[ApiController]
[Route("api/[controller]/")]
public class ContactController : ControllerBase {


    /// <summary>
    /// Cria um novo contato.
    /// </summary>
    /// <param name="contactRequest">O objeto de solicitação de criação do contato.</param>
    /// <response code="200">Retorna o novo contato criado.</response>
    /// <response code="400">Solicitação inválida.</response>
    /// <response code="500">Houve um erro interno no servidor.</response>
    /// <returns>Uma resposta de contato recém-criada.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ContactRequest contactRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var factory = new ConnectionFactory() {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
            Port = 5672,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            AutomaticRecoveryEnabled = true
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchange: ExchangeConfiguration.Name, type: "direct", durable: true, autoDelete: false, arguments: null);
        await channel.QueueDeclareAsync(queue: QueueConfiguration.ContactCreatedQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        await channel.QueueBindAsync(queue: QueueConfiguration.ContactCreatedQueue, exchange: ExchangeConfiguration.Name, 
            routingKey: RoutingKeyConfiguration.RoutingQueueCreate, arguments: null);
        
        var message = JsonSerializer.Serialize(contactRequest);
        var body = Encoding.UTF8.GetBytes(message);
        
        await channel.BasicPublishAsync(
            exchange: ExchangeConfiguration.Name, 
            routingKey: RoutingKeyConfiguration.RoutingQueueCreate, 
            mandatory: true, 
            body);
        
        //retorno da mensagem

        return Ok("Solicitação de criação de usuário efetuada com sucesso.");
        
        //var response = _contactInterface.Create(contactRequest);
        //return response.IsSuccess ? Ok(response) : Problem(null, null, 500, response.Message);
    }
}
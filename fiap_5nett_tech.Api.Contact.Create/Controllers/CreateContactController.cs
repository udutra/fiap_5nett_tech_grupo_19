using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using fiap_5nett_tech.Application.DataTransfer.Request;
using fiap_5nett_tech.Application.DataTransfer.Response;
using fiap_5nett_tech.Domain.RabbitMqConfiguration;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace fiap_5nett_tech.Api.Contact.Create.Controllers;

[ApiController]
[Route("api/[controller]/")]
public class CreateContactController : ControllerBase {
    
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly IConnectionFactory connectionFactory;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper = new();
    private const string _replyQueueName = QueueConfiguration.ContactCreatedQueueReturn;
    
    public CreateContactController()
    {
        connectionFactory = new ConnectionFactory  {
            Uri = new Uri(@"amqp://guest:guest@127.0.0.1:5672/"),
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            AutomaticRecoveryEnabled = true
        };
        
        _connection = connectionFactory.CreateConnectionAsync().Result;
        _channel = _connection.CreateChannelAsync().Result;
    }
    
    private async Task StartConsumerAsync()
    {
        if (_channel is null){
            throw new InvalidOperationException();
        }
        
        await _channel.QueueDeclareAsync(queue: _replyQueueName, durable: false, exclusive: true, autoDelete: true, arguments: null);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        
        consumer.ReceivedAsync += (model, ea) =>
        {
            var correlationId = ea.BasicProperties.CorrelationId;

            if (string.IsNullOrEmpty(correlationId) == false)
            {
                if (_callbackMapper.TryRemove(correlationId, out var tcs))
                {
                    var body = ea.Body.ToArray();
                    var response = Encoding.UTF8.GetString(body);
                    tcs.TrySetResult(response);
                }
            }
            return Task.CompletedTask;
        };

        await _channel.BasicConsumeAsync(_replyQueueName, true, consumer);
    }


    /// <summary>
    /// Cria um novo contato.
    /// </summary>
    /// <param name="contactRequest">O objeto de solicitação de criação do contato.</param>
    /// <response code="200">Retorna o novo contato criado.</response>
    /// <response code="400">Solicitação inválida.</response>
    /// <response code="500">Houve um erro interno no servidor.</response>
    /// <returns>Uma resposta de contato recém-criada.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ContactRequest contactRequest, CancellationToken cancellationToken = default)
    {
        
        if (!ModelState.IsValid)
        {
            return StatusCode(StatusCodes.Status400BadRequest, "Campo(s) inválido(s) - BadRequest");
        }
        if (_channel is null){
            throw new InvalidOperationException();
        }
        
        await StartConsumerAsync();
        try
        {
            await _channel.ExchangeDeclareAsync(exchange: ExchangeConfiguration.Name, type: "direct", durable: true, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
            await _channel.QueueDeclareAsync(queue: QueueConfiguration.ContactCreatedQueue, durable: false, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(queue: QueueConfiguration.ContactCreatedQueue, exchange: ExchangeConfiguration.Name, routingKey: RoutingKeyConfiguration.RoutingQueueCreate, arguments: null, cancellationToken: cancellationToken);
            
            var correlationId = Guid.NewGuid().ToString();
            var props = new BasicProperties
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                DeliveryMode = DeliveryModes.Persistent,
                CorrelationId = correlationId,
                ReplyTo = _replyQueueName
            };
            
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            _callbackMapper.TryAdd(correlationId, tcs);
            
            var message = JsonSerializer.Serialize(contactRequest);
            var body = Encoding.UTF8.GetBytes(message);

            await _channel.BasicPublishAsync(
                exchange: ExchangeConfiguration.Name,
                routingKey: RoutingKeyConfiguration.RoutingQueueCreate,
                mandatory: true,
                basicProperties: props,
                body, 
                cancellationToken: cancellationToken);

            //retorno da mensagem
            await using var ctr = cancellationToken.Register(() => {
                _callbackMapper.TryRemove(correlationId, out _);
                tcs.SetCanceled(cancellationToken);
            });

            var result = tcs.Task.Result;
            
            var response = JsonSerializer.Deserialize<ContactResponse<Domain.Entities.Contact>>(result);
            
            return response.IsSuccess ? Ok(response) : Problem(null, null, 500, response.Message);
        }
        catch (BrokerUnreachableException e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro interno do servidor - BrokerUnreachableException");
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro interno do servidor - Exception" + e.Message);
        }
    }

    private async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync();
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync();
        }
    }
}
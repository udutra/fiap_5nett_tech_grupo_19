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

namespace fiap_5nett_tech.Api.Contact.Read.Controllers;

/// <summary>
/// 
/// </summary>
[ApiController]
[Route("api/[controller]/")]
public class ReadContactController : ControllerBase
{
    private readonly IConnection? _connection;
    private readonly IChannel? _channel;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper = new();
    private const string ReplyQueueNameGetOnById = QueueConfiguration.ContactReadQueueGetOneByIdReturn;
    private const string ReplyQueueNameGetOnByDddAndPhone = QueueConfiguration.ContactReadQueueGetOneByDddAndPhoneReturn;
    private const string ReplyQueueNameGetAll = QueueConfiguration.ContactReadQueueGetAllReturn;

    /// <summary>
    /// 
    /// </summary>
    public ReadContactController()
    {
        var connectionFactory = new ConnectionFactory
        {
            Uri = new Uri(@"amqp://guest:guest@localhost:5672/"),
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            AutomaticRecoveryEnabled = true
        };

        _connection = connectionFactory.CreateConnectionAsync().Result;
        _channel = _connection.CreateChannelAsync().Result;
    }

    private async Task StartConsumerAsync(string rqn)
    {
        if (_channel is null)
        {
            throw new InvalidOperationException();
        }

        await _channel.QueueDeclareAsync(queue: rqn, durable: false, exclusive: true, autoDelete: true,
            arguments: null);

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

        await _channel.BasicConsumeAsync(rqn, true, consumer);
    }

    /// <summary>
    /// Obtém um contato pelo DDD e número de telefone.
    /// </summary>
    /// <param name="id">Id do contato</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <response code="200">Retorna o contato se encontrado.</response>
    /// <response code="400">Se o contato não for encontrado.</response>
    /// <response code="500">Houve um erro interno no servidor.</response>
    /// <returns>Um objeto de resposta de contato.</returns>
    [HttpGet]
    [Route("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ContactResponse<Domain.Entities.Contact?>>> GetOne([FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) {
            return StatusCode(StatusCodes.Status400BadRequest, "Campo(s) inválido(s) - BadRequest");
        }

        if (_channel is null) {
            throw new InvalidOperationException();
        }

        await StartConsumerAsync(ReplyQueueNameGetOnById);
        try
        {
            await _channel.ExchangeDeclareAsync(exchange: ExchangeConfiguration.Name, type: "direct", durable: true,
                autoDelete: false, arguments: null,
                cancellationToken: cancellationToken);
            await _channel.QueueDeclareAsync(queue: QueueConfiguration.ContactReadQueueGetOneById, durable: false,
                exclusive: false, autoDelete: false, arguments: null,
                cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(queue: QueueConfiguration.ContactReadQueueGetOneById,
                exchange: ExchangeConfiguration.Name,
                routingKey: RoutingKeyConfiguration.RoutingQueueReadGetOneById, arguments: null,
                cancellationToken: cancellationToken);

            var correlationId = Guid.NewGuid().ToString();
            var props = new BasicProperties
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                DeliveryMode = DeliveryModes.Persistent,
                CorrelationId = correlationId,
                ReplyTo = ReplyQueueNameGetOnById
            };

            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            _callbackMapper.TryAdd(correlationId, tcs);
            
            var message = JsonSerializer.Serialize(id);
            var body = Encoding.UTF8.GetBytes(message);

            await _channel.BasicPublishAsync(
                exchange: ExchangeConfiguration.Name,
                routingKey: RoutingKeyConfiguration.RoutingQueueReadGetOneById,
                mandatory: true,
                basicProperties: props,
                body,
                cancellationToken: cancellationToken);

            //retorno da mensagem
            await using var ctr = cancellationToken.Register(() =>
            {
                _callbackMapper.TryRemove(correlationId, out _);
                tcs.SetCanceled(cancellationToken);
            });

            try
            {
                var response = JsonSerializer.Deserialize<ContactResponse<Domain.Entities.Contact>>(tcs.Task.Result);
                await DisposeAsync();
                if (response == null)
                {
                    return Problem(null, null, 500, "Erro interno do servidor - Response nulo");
                }

                return response.IsSuccess ? Ok(response) : Problem(null, null, 500, response.Message);
            }
            catch (ArgumentNullException e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Erro interno do servidor - ArgumentNullException" + e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Erro interno do servidor - Exception" + e.Message);
            }
        }
        catch (BrokerUnreachableException e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro interno do servidor - BrokerUnreachableException" + e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro interno do servidor - Exception" + e.Message);
        }
    }

    /// <summary>
    /// Obtém um contato pelo DDD e número de telefone.
    /// </summary>
    /// <param name="ddd">DDD do contato.</param>
    /// <param name="telefone">Número de telefone do contato.</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <response code="200">Retorna o contato se encontrado.</response>
    /// <response code="400">Se o contato não for encontrado.</response>
    /// <response code="500">Houve um erro interno no servidor.</response>
    /// <returns>Um objeto de resposta de contato.</returns>
    [HttpGet]
    [Route("{ddd:int}/{telefone}")]
    public async Task<ActionResult<ContactResponse<Domain.Entities.Contact?>>> GetOne([FromRoute] int ddd,
        [FromRoute] string telefone, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) {
            return StatusCode(StatusCodes.Status400BadRequest, "Campo(s) inválido(s) - BadRequest");
        }

        if (_channel is null) {
            throw new InvalidOperationException();
        }

        await StartConsumerAsync(ReplyQueueNameGetOnByDddAndPhone);
        try
        {
            await _channel.ExchangeDeclareAsync(exchange: ExchangeConfiguration.Name, type: "direct", durable: true,
                autoDelete: false, arguments: null,
                cancellationToken: cancellationToken);
            await _channel.QueueDeclareAsync(queue: QueueConfiguration.ContactReadQueueGetOneByDddAndPhone, durable: false,
                exclusive: false, autoDelete: false, arguments: null,
                cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(queue: QueueConfiguration.ContactReadQueueGetOneByDddAndPhone,
                exchange: ExchangeConfiguration.Name,
                routingKey: RoutingKeyConfiguration.RoutingQueueReadGetOneByDddAndPhone, arguments: null,
                cancellationToken: cancellationToken);

            var correlationId = Guid.NewGuid().ToString();
            var props = new BasicProperties
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                DeliveryMode = DeliveryModes.Persistent,
                CorrelationId = correlationId,
                ReplyTo = ReplyQueueNameGetOnByDddAndPhone
            };

            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            _callbackMapper.TryAdd(correlationId, tcs);
            
            var contact = new Domain.Entities.Contact
            {
                Ddd = ddd,
                Phone = telefone
            };
            
            var message = JsonSerializer.Serialize(contact);
            var body = Encoding.UTF8.GetBytes(message);

            await _channel.BasicPublishAsync(
                exchange: ExchangeConfiguration.Name,
                routingKey: RoutingKeyConfiguration.RoutingQueueReadGetOneByDddAndPhone,
                mandatory: true,
                basicProperties: props,
                body,
                cancellationToken: cancellationToken);

            //retorno da mensagem
            await using var ctr = cancellationToken.Register(() =>
            {
                _callbackMapper.TryRemove(correlationId, out _);
                tcs.SetCanceled(cancellationToken);
            });

            try
            {
                var response = JsonSerializer.Deserialize<ContactResponse<Domain.Entities.Contact?>?>(tcs.Task.Result);
                await DisposeAsync();
                if (response == null)
                {
                    return Problem(null, null, 500, "Erro interno do servidor - Response nulo");
                }

                return response.IsSuccess ? Ok(response) : Problem(null, null, 500, response.Message);
            }
            catch (ArgumentNullException e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Erro interno do servidor - ArgumentNullException" + e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Erro interno do servidor - Exception" + e.Message);
            }
        }
        catch (BrokerUnreachableException e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro interno do servidor - BrokerUnreachableException" + e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro interno do servidor - Exception" + e.Message);
        }
    }


    /// <summary>
    /// Obtém todos os contatos
    /// </summary>
    /// <param name="contactRequest">O objeto de solicitação para buscar todos os contatos.</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <response code="200">Retorna uma lista paginada de contatos.</response>
    /// <response code="400">Solicitação inválida.</response>
    /// <response code="500">Houve um erro interno no servidor.</response>
    /// <returns>Um objeto de resposta de contato paginado.</returns>
    [HttpGet]
    public async Task<PagedContactResponse<List<Domain.Entities.Contact>>> GetAll([FromQuery] GetAllContactRequest contactRequest,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return new PagedContactResponse<List<Domain.Entities.Contact>>(null, 400, "BadRequest");
        }

        if (_channel is null) {
            throw new InvalidOperationException();
        }

        await StartConsumerAsync(ReplyQueueNameGetAll);
        try
        {
            await _channel.ExchangeDeclareAsync(exchange: ExchangeConfiguration.Name, type: "direct", durable: true,
                autoDelete: false, arguments: null,
                cancellationToken: cancellationToken);
            await _channel.QueueDeclareAsync(queue: QueueConfiguration.ContactReadQueueGetAll, durable: false,
                exclusive: false, autoDelete: false, arguments: null,
                cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(queue: QueueConfiguration.ContactReadQueueGetAll,
                exchange: ExchangeConfiguration.Name,
                routingKey: RoutingKeyConfiguration.RoutingQueueReadGetAll, arguments: null,
                cancellationToken: cancellationToken);

            var correlationId = Guid.NewGuid().ToString();
            var props = new BasicProperties
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                DeliveryMode = DeliveryModes.Persistent,
                CorrelationId = correlationId,
                ReplyTo = ReplyQueueNameGetAll
            };

            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            _callbackMapper.TryAdd(correlationId, tcs);
            
            var message = JsonSerializer.Serialize(contactRequest);
            var body = Encoding.UTF8.GetBytes(message);

            await _channel.BasicPublishAsync(
                exchange: ExchangeConfiguration.Name,
                routingKey: RoutingKeyConfiguration.RoutingQueueReadGetAll,
                mandatory: true,
                basicProperties: props,
                body,
                cancellationToken: cancellationToken);

            //retorno da mensagem
            await using var ctr = cancellationToken.Register(() =>
            {
                _callbackMapper.TryRemove(correlationId, out _);
                tcs.SetCanceled(cancellationToken);
            });

            try
            {
                var response = JsonSerializer.Deserialize<PagedContactResponse<List<Domain.Entities.Contact>>>(tcs.Task.Result);
                await DisposeAsync();
                return response ?? 
                       new PagedContactResponse<List<Domain.Entities.Contact>>(null, 500, 
                           "Erro interno do servidor - Response nulo");
            }
            catch (ArgumentNullException e)
            {
                return new PagedContactResponse<List<Domain.Entities.Contact>>(null, 500, 
                    "Erro interno do servidor - ArgumentNullException" + e.Message);
            }
            catch (Exception e)
            {
                return new PagedContactResponse<List<Domain.Entities.Contact>>(null, 500, 
                    "Erro interno do servidor - Exception" + e.Message);
            }
        }
        catch (BrokerUnreachableException e)
        {
            return new PagedContactResponse<List<Domain.Entities.Contact>>(null, 500, 
                "Erro interno do servidor - BrokerUnreachableException" + e.Message);
        }
        catch (Exception e)
        {
            return new PagedContactResponse<List<Domain.Entities.Contact>>(null, 500, 
                "Erro interno do servidor - Exception" + e.Message);
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
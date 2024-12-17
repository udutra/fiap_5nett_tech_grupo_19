using System.Text;
using System.Text.Json;
using fiap_5nett_tech.Application.DataTransfer.Request;
using fiap_5nett_tech.Application.Interface;
using fiap_5nett_tech.Domain.RabbitMqConfiguration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace fiap_5nett_tech.Api.Contact.Read.Services;

/// <summary>
/// 
/// </summary>
public class WorkerGetAll : IDisposable
{
    private readonly IConnection? _connection;
    private readonly IChannel? _channel;
    private readonly IContactInterface _contactService;
    private readonly IServiceScopeFactory ServiceScopeFactory;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="contactService"></param>
    /// <param name="serviceScopeFactory"></param>
    public WorkerGetAll(IContactInterface contactService, IServiceScopeFactory serviceScopeFactory)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
            Port = 5672,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            AutomaticRecoveryEnabled = true
        };

        _connection = factory.CreateConnectionAsync().Result;
        _channel = _connection.CreateChannelAsync().Result;

        _channel.ExchangeDeclareAsync(exchange: ExchangeConfiguration.Name, type: "direct", durable: true,
            autoDelete: false, arguments: null);
        
        _channel.QueueDeclareAsync(queue: QueueConfiguration.ContactReadQueueGetAll, durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBindAsync(queue: QueueConfiguration.ContactReadQueueGetAll, exchange: ExchangeConfiguration.Name, 
            routingKey: RoutingKeyConfiguration.RoutingQueueReadGetAll, arguments: null);
        
        _contactService = contactService;
        ServiceScopeFactory = serviceScopeFactory;
    }
    
    public async Task Start()
    {
        if (_channel is null){
            throw new InvalidOperationException();
        }
        
        await _channel.BasicQosAsync(0, 1, false);
        
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var props = ea.BasicProperties;
            var replyProps = new BasicProperties
            {
                CorrelationId = props.CorrelationId
            };

            GetAllContactRequest? contact;
            
            try
            {
                contact = JsonSerializer.Deserialize<GetAllContactRequest>(message);
                if (contact == null)
                {
                    throw new NullReferenceException("Contato não pode ser nulo.");
                }
            }
            catch (NullReferenceException ex)
            {
                await _channel.BasicRejectAsync(deliveryTag: ea.DeliveryTag, requeue: false);
                throw;
            }
            catch (Exception ex)
            {
                await _channel.BasicRejectAsync(deliveryTag: ea.DeliveryTag, requeue: false);
                throw;
            }
            
            try
            {
                var contactResponse = _contactService.GetAll(contact);
                var m = JsonSerializer.Serialize(contactResponse);
                var responseBytes = Encoding.UTF8.GetBytes(m);
                await _channel.BasicPublishAsync(exchange: string.Empty, routingKey: props.ReplyTo!, mandatory: true,
                    basicProperties: replyProps, body: responseBytes);
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            }catch (AlreadyClosedException ex)
            {
                await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                throw;
            }
            catch (Exception ex)
            {
                await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                throw;
            }

            await Task.CompletedTask;
        };
        await _channel.BasicConsumeAsync(queue: QueueConfiguration.ContactReadQueueGetAll, autoAck: false,
            consumer: consumer);
    }
    
    
    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
        _channel?.CloseAsync();

        _connection?.CloseAsync();
    }
}
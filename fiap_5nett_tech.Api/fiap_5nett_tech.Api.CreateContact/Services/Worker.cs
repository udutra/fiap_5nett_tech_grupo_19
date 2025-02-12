using System.Text;
using System.Text.Json;
using fiap_5nett_tech.Application.DataTransfer.Request;
using fiap_5nett_tech.Application.Interface;
using fiap_5nett_tech.Domain.RabbitMqConfiguration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace fiap_5nett_tech.Api.CreateContact.Services;

public class Worker : IDisposable
{
    private readonly IConnection? _connection;
    private readonly IChannel? _channel;
    private readonly IContactInterface ContactService;
    private readonly IServiceScopeFactory ServiceScopeFactory;

    public Worker(IContactInterface contactService, IServiceScopeFactory serviceScopeFactory)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri("amqp://guest:guest@rabbitmq-service:5672/"),
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            AutomaticRecoveryEnabled = true
        };

        _connection = factory.CreateConnectionAsync().Result;
        _channel = _connection.CreateChannelAsync().Result;

        _channel.ExchangeDeclareAsync(exchange: ExchangeConfiguration.Name, type: "direct", durable: true,
            autoDelete: false, arguments: null);
        _channel.QueueDeclareAsync(queue: QueueConfiguration.ContactCreatedQueue, durable: false, exclusive: false,
            autoDelete: false, arguments: null);
        _channel.QueueBindAsync(queue: QueueConfiguration.ContactCreatedQueue, exchange: ExchangeConfiguration.Name,
            routingKey: RoutingKeyConfiguration.RoutingQueueCreate, arguments: null);

        ContactService = contactService;
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

            var contactRequest = JsonSerializer.Deserialize<ContactRequest>(message);

            try
            {
                if (contactRequest == null)
                {
                    throw new NullReferenceException("Contato não pode ser nulo.");
                }
                //Console.WriteLine("contato lido");
            }
            catch (NullReferenceException ex)
            {

                //Se der erro, reenvia a mensagem para a fila
                //await _channel.BasicNacksAsync(eventArgs.DeliveryTag, false, );
                await _channel.BasicRejectAsync(deliveryTag: ea.DeliveryTag, requeue: false);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetBaseException());
                Console.WriteLine(ex.Message);
                await _channel.BasicRejectAsync(deliveryTag: ea.DeliveryTag, requeue: false);
                throw;
            }

            try
            {
                var contactResponse = ContactService.Create(contactRequest);
                var m = JsonSerializer.Serialize(contactResponse);
                var responseBytes = Encoding.UTF8.GetBytes(m);
                await _channel.BasicPublishAsync(exchange: string.Empty, routingKey: props.ReplyTo!, mandatory: true,
                    basicProperties: replyProps, body: responseBytes);
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (AlreadyClosedException ex)
            {
                //Log
                Console.WriteLine("AlreadyClosedException");
                Console.WriteLine(ex.Message);
                await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                throw;
            }
            catch (Exception ex)
            {
                //Log
                Console.WriteLine(ex.GetBaseException());
                Console.WriteLine(ex.Message);
                await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                throw;
            }

            await Task.CompletedTask;
        };

        await _channel.BasicConsumeAsync(queue: QueueConfiguration.ContactCreatedQueue, autoAck: false,
            consumer: consumer);
    }
    
    public void Dispose()
    {
        _channel?.CloseAsync();

        _connection?.CloseAsync();
    }
}
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using fiap_5nett_tech.Application.DataTransfer.Request;
using fiap_5nett_tech.Application.Interface;
using fiap_5nett_tech.Domain.RabbitMqConfiguration;
using fiap_5nett_tech.Domain.Repositories;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace fiap_5nett_tech.Api.Contact.Create.Consumer;

public class Worker : IDisposable {
    
    private readonly IConnection connection;
    private readonly IChannel channel;
    private readonly IContactInterface ContactService;
    private readonly IServiceScopeFactory ServiceScopeFactory;
    
    public Worker(IContactInterface contactService, IServiceScopeFactory serviceScopeFactory)
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
        
        connection = factory.CreateConnectionAsync().Result;
        channel = connection.CreateChannelAsync().Result;
        
        channel.ExchangeDeclareAsync(exchange: ExchangeConfiguration.Name, type: "direct", durable: true, autoDelete: false, arguments: null);
        channel.QueueDeclareAsync(queue: QueueConfiguration.ContactCreatedQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueBindAsync(queue: QueueConfiguration.ContactCreatedQueue, exchange: ExchangeConfiguration.Name, 
            routingKey: RoutingKeyConfiguration.RoutingQueueCreate, arguments: null);
        
        ContactService = contactService;
        ServiceScopeFactory = serviceScopeFactory;
    } 
    
    public async Task Start() {

            //channel.BasicQosAsync(0, 1, false);
            await channel.BasicQosAsync(0, 1, false);
            
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) => {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var props = ea.BasicProperties;
                var replyProps = new BasicProperties {
                    CorrelationId = props.CorrelationId
                };

                var contactRequest = JsonSerializer.Deserialize<ContactRequest>(message);

                try
                {
                    if (contactRequest == null)
                    {
                        throw new NullReferenceException("Contato não pode ser nulo.");
                    }
                    
                    //await channel.BasicAckAsync(eventArgs.DeliveryTag, false);
                    Console.WriteLine("contato lido");
                }
                catch (NullReferenceException ex) {
                    
                    //Se der erro, reenvia a mensagem para a fila
                    //await channel.BasicNacksAsync(eventArgs.DeliveryTag, false, );
                    await channel.BasicRejectAsync(deliveryTag: ea.DeliveryTag, requeue: false);
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.GetBaseException());
                    Console.WriteLine(ex.Message);
                    await channel.BasicRejectAsync(deliveryTag: ea.DeliveryTag, requeue: false);
                    throw;
                }
                
                try
                {
                    var contactResponse = ContactService.Create(contactRequest);
                    var m = JsonSerializer.Serialize(contactResponse);
                    var responseBytes = Encoding.UTF8.GetBytes(m);
                    await channel.BasicPublishAsync(exchange: string.Empty, routingKey: props.ReplyTo!, mandatory: true, basicProperties: replyProps, body: responseBytes);
                    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (AlreadyClosedException ex)
                {
                    //Log
                    Console.WriteLine("AlreadyClosedException");
                    Console.WriteLine(ex.Message);
                    await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    throw;
                }
                catch (Exception ex)
                {
                    //Log
                    Console.WriteLine(ex.GetBaseException());
                    Console.WriteLine(ex.Message);
                    await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    throw;
                }
                
                await Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(queue: QueueConfiguration.ContactCreatedQueue, autoAck: false, consumer: consumer);
    }
  

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
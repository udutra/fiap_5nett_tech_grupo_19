using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using fiap_5nett_tech.Application.DataTransfer.Request;
using fiap_5nett_tech.Application.Interface;
using fiap_5nett_tech.Domain.RabbitMqConfiguration;
using fiap_5nett_tech.Domain.Repositories;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace fiap_5nett_tech.Api.Contact.Create.Consumer;

public class Worker: IDisposable {
    
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

            ContactRequest? contactRequest;
            
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, eventArgs) => {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                contactRequest = JsonSerializer.Deserialize<ContactRequest>(message);

                try
                {
                    if (contactRequest == null)
                    {
                        throw new NullReferenceException("Contato não pode ser nulo.");
                    }
                    
                    ContactService.Create(contactRequest);
                    //await channel.BasicAckAsync(eventArgs.DeliveryTag, false);
                    Console.WriteLine("contato lido");
                }
                catch (NullReferenceException ex) {
                    
                    //Se der erro, reenvia a mensagem para a fila
                    //await channel.BasicNacksAsync(eventArgs.DeliveryTag, false, );
                }
                
                await Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(queue: QueueConfiguration.ContactCreatedQueue, autoAck: true, consumer: consumer);
    }

    public void Dispose()
    {
        channel.CloseAsync();
        connection.CloseAsync();
    }
}
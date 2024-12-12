using System.Text;
using System.Text.Json;
using fiap_5nett_tech.Application;
using fiap_5nett_tech.Application.DataTransfer.Response;
using fiap_5nett_tech.Domain.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace fiap_5nett_tech.api;

public class Worker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested) {
            var factory = new ConnectionFactory() {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            
            using var connection = factory.CreateConnectionAsync(stoppingToken);
            await using var channel = connection.Result.CreateChannelAsync(cancellationToken: stoppingToken).Result;
            
            await channel.QueueDeclareAsync(queue: QueueConfiguration.ContactCreatedQueue, durable: false, exclusive: false, autoDelete: false, 
                arguments: null, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            
            consumer.ReceivedAsync += (_, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var usuario = JsonSerializer.Deserialize<ContactResponse<Contact>>(message);
                Console.WriteLine(usuario?.ToString());
                return Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(queue: QueueConfiguration.ContactCreatedQueue, autoAck: true, consumer: consumer, 
                cancellationToken: stoppingToken);
        }
        await Task.Delay(2000, stoppingToken);
    }
}
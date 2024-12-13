using System.Text;
using fiap_5nett_tech.Application;
using fiap_5nett_tech.Application.DataTransfer.Request;
using fiap_5nett_tech.Application.DataTransfer.Response;
using fiap_5nett_tech.Domain.Entities;
using fiap_5nett_tech.Domain.Repositories;
using Elastic.Apm;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;

namespace fiap_5nett_tech.api.Consumers;

public class RabbitMqAddUserConsumer
{
    private readonly Task<IConnection> _connection;
    private readonly IChannel _channel; 
    private readonly IContactRepository _contact;
    private readonly IRegionRepository _region;
    //private readonly ILogger _ilogger;


    public RabbitMqAddUserConsumer(IContactRepository contact, IRegionRepository region) //ILogger ilogger)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnectionAsync();
        _channel = _connection.Result.CreateChannelAsync().Result;
        _channel.QueueDeclareAsync(queue: QueueConfiguration.ContactCreatedQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);

        _contact = contact;
        _region = region;
        //_ilogger = ilogger;
    }

    public void Start()
    {
        //_ilogger.LogInformation("Iniciada a execução");
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            ContactResponse<Contact> response;

            try
            {
                var request = JsonSerializer.Deserialize<ContactRequest>(message) ?? throw new ArgumentNullException();
                
                // Validações
                var region = _region.GetOne(request.Ddd);
                if (region == null)
                {
                   // _ilogger.LogError("não existe");
                    throw new Exception("Região não encontrada!");
                    
                }

                if (_contact.GetOne(request.Ddd, request.PhoneNumber) != null)
                {
                    throw new Exception("Telefone já cadastrado!");
                }

                if (request.PhoneNumber.Length != 9)
                {
                    throw new Exception("Quantidade de caracteres de telefone inválido!");
                }

                // Persistir no banco de dados
                var contact = new Contact(request.Name, request.Email, request.PhoneNumber, region);
                _contact.Create(contact);

                // Resposta de sucesso
                response = new ContactResponse<Contact>(contact, 200, "Contato criado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar mensagem: {ex.Message}");

                response = new ContactResponse<Contact>(null, 400, ex.Message);

                await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                return;
            }

            // Publicar resposta
            var props = ea.BasicProperties;
            var replyProps = new BasicProperties
            {
                CorrelationId = props.CorrelationId
            };

            var cr = JsonSerializer.Serialize(response);
            var responseBytes = Encoding.UTF8.GetBytes(cr);

            await _channel.BasicPublishAsync(exchange: string.Empty, routingKey: props.ReplyTo!,
                mandatory: true, basicProperties: replyProps, body: responseBytes);

            // Confirmar processamento
            await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
        };

        // Registrar consumidor na fila
        _channel.BasicConsumeAsync(queue: QueueConfiguration.ContactCreatedQueue, autoAck: false, consumer: consumer);
    }

    internal object Stop()
    {
        throw new NotImplementedException();
    }
}
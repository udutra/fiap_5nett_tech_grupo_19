using System.Text;
using fiap_5nett_tech.Application;
using fiap_5nett_tech.Application.DataTransfer.Request;
using fiap_5nett_tech.Application.DataTransfer.Response;
using fiap_5nett_tech.Domain.Entities;
using fiap_5nett_tech.Domain.Repositories;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace fiap_5nett_tech.api.Consumers;

public class RabbitMqAddUserConsumer
{
    private readonly Task<IConnection> _connection;
    private readonly IChannel _channel; 
    private readonly IContactRepository _contact;
    private readonly IRegionRepository _region;
    
    
    public RabbitMqAddUserConsumer(IContactRepository contact, IRegionRepository region)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnectionAsync();
        _channel = _connection.Result.CreateChannelAsync().Result;
        _channel.QueueDeclareAsync(queue: QueueConfiguration.ContactCreatedQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        
        _contact = contact;
        _region = region;
    }


    public void Start()
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        ContactResponse<Contact> response;
        
        consumer.ReceivedAsync += async (_, ea) => {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            ContactRequest request;
            
            try
            {
                request = JsonSerializer.Deserialize<ContactRequest>(message) ?? throw new ArgumentNullException();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await _channel.BasicRejectAsync(deliveryTag: ea.DeliveryTag, requeue: true);
                throw;
            }

            try
            {
                
                var region = _region.GetOne(request.Ddd);
                
                if (region == null)
                {
                    response = new ContactResponse<Contact>(null, 400, "Região não encontrada!");
                }

                if (_contact.GetOne(request.Ddd, request.PhoneNumber) != null)
                {
                    response = new ContactResponse<Contact>(null, 400, "Telefone já Cadastrado!");
                }

                if (request.PhoneNumber.Length != 9)
                {
                    response = new ContactResponse<Contact>(null, 400, "Quantidade de caracteres de telefone invalido!");
                }

                Contact contact = new(request.Name, request.Email, request.PhoneNumber, region);
                _contact.Create(contact);
                response = new ContactResponse<Contact>(contact, 200, "Contato criado com sucesso!");
                
                var props = ea.BasicProperties;
                var replyProps = new BasicProperties
                {
                    CorrelationId = props.CorrelationId
                };
                
                var cr = JsonConvert.SerializeObject(response);
                var responseBytes = Encoding.UTF8.GetBytes(cr);
                
                await _channel.BasicPublishAsync(exchange: string.Empty, routingKey: props.ReplyTo!, mandatory: true, basicProperties: replyProps, 
                    body: responseBytes);
                
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                //Log
                Console.WriteLine(ex.Message);
                
                response = new ContactResponse<Contact>(null, 500, "Não foi possível criar o contato!");
                
                var props = ea.BasicProperties;
                var replyProps = new BasicProperties
                {
                    CorrelationId = props.CorrelationId
                };
                
                var cr = JsonConvert.SerializeObject(response);
                var responseBytes = Encoding.UTF8.GetBytes(cr);
                
                await _channel.BasicPublishAsync(exchange: string.Empty, routingKey: props.ReplyTo!,
                    mandatory: true, basicProperties: replyProps, body: responseBytes);
                
                await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                
                throw;
                
            }
            await _channel.BasicConsumeAsync(queue: QueueConfiguration.ContactCreatedQueue, autoAck: false, consumer: consumer);
        };
    }
}
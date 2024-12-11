using System.Text;
using System.Text.Json;
using fiap_5nett_tech.Application.DataTransfer.Request;
using fiap_5nett_tech.Application.Interface;
using fiap_5nett_tech.Domain.Entities;
using fiap_5nett_tech.Application.DataTransfer.Response;
using fiap_5nett_tech.Domain.Repositories;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace fiap_5nett_tech.Application.Service
{
    public class ContactService : IContactInterface
    {

        private readonly IContactRepository _contact;
        private readonly IRegionRepository _region;

        public ContactService(IContactRepository contact, IRegionRepository region)
        {
            _contact = contact;
            _region = region;
        }

        public async Task<ContactResponse<Contact?>> Create() {
            var factory = new ConnectionFactory() {
                HostName = "localhost",
                UserName = "username",
                Password = "password",
                VirtualHost = "/",
                Port = 5672,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                AutomaticRecoveryEnabled = true
            };

            using var connection = factory.CreateConnectionAsync();
            var channel = connection.Result.CreateChannelAsync().Result;
        
            await channel.ExchangeDeclareAsync(exchange: ExchangeConfiguration.Name, type: "direct", durable: true, autoDelete: false, arguments: null);
            await channel.QueueDeclareAsync(queue: QueueConfiguration.ContactCreatedQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
            await channel.QueueBindAsync(queue: QueueConfiguration.ContactCreatedQueue, exchange: ExchangeConfiguration.Name, 
                routingKey: RoutingKeyConfiguration.RoutingQueueCreate, arguments: null);
        
            var consumer = new AsyncEventingBasicConsumer(channel);

            var response = new ContactResponse<Contact?>();
            
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
                    await channel.BasicRejectAsync(deliveryTag: ea.DeliveryTag, requeue: true);
                    throw;
                }

                try
                {
                    
                    var region = _region.GetOne(request.Ddd);
                    
                    if (region == null)
                    {
                        response = new ContactResponse<Contact?>(null, 400, "Região não encontrada!");
                    }

                    if (_contact.GetOne(request.Ddd, request.PhoneNumber) != null)
                    {
                        response = new ContactResponse<Contact?>(null, 400, "Telefone já Cadastrado!");
                    }

                    if (request.PhoneNumber.Length != 9)
                    {
                        response = new ContactResponse<Contact?>(null, 400, "Quantidade de caracteres de telefone invalido!");
                    }

                    Contact contact = new(request.Name, request.Email, request.PhoneNumber, region);
                    _contact.Create(contact);
                    response = new ContactResponse<Contact?>(contact, 200, "Contato criado com sucesso!");
                    
                    
                    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    //Log
                    Console.WriteLine(ex.Message);
                    await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    response = new ContactResponse<Contact?>(null, 500, "Não foi possível criar o contato!");
                    throw;
                    
                }
                await channel.BasicConsumeAsync(queue: QueueConfiguration.ContactCreatedQueue, autoAck: false, consumer: consumer);
            };
            return response;
        }

        public ContactResponse<Contact?> Update(ContactRequest contactRequest)
        {
            try
            {
                var contact = _contact.GetOne(contactRequest.Ddd, contactRequest.PhoneNumber);

                if (contact is null)
                    return new ContactResponse<Contact?>(null, 404, "Contato não encontrado!");

                if (!string.IsNullOrEmpty(contactRequest.Name))
                    contact.Name = contactRequest.Name;

                if (!string.IsNullOrEmpty(contactRequest.Email))
                    contact.Email = contactRequest.Email;

                _contact.Update(contact);

                return new ContactResponse<Contact?>(contact, message: "Contato atualizado com sucesso!");
            }
            catch
            {
                return new ContactResponse<Contact?>(null, 500, "Não foi possível atualizar o contato!");
            }
        }

        public ContactResponse<Contact?> GetOne(Guid id)
        {
            try
            {
                var contact = _contact.GetOne(id);

                return contact is null
                    ? new ContactResponse<Contact?>(null, 404, "Contato não encontrado!")
                    : new ContactResponse<Contact?>(contact);
            }
            catch
            {
                return new ContactResponse<Contact?>(null, 500, "Não foi possível recuperar o Contato!");
            }
        }

        public ContactResponse<Contact?> GetOne(int ddd, string telefone)
        {
            try
            {
                var contact = _contact.GetOne(ddd, telefone);

                return contact is null
                    ? new ContactResponse<Contact?>(null, 404, "Contato não encontrado!")
                    : new ContactResponse<Contact?>(contact);
            }
            catch
            {
                return new ContactResponse<Contact?>(null, 500, "Não foi possível recuperar o Contato!");
            }
        }

        public ContactResponse<Contact?> Delete(int ddd, string telefone)
        {
            try
            {
                var contact = _contact.Delete(ddd, telefone);

                return contact is null
                    ? new ContactResponse<Contact?>(null, 404, "Contato não encontrado!")
                    : new ContactResponse<Contact?>(contact, 200, "Contato excluído com sucesso!");
            }
            catch
            {
                return new ContactResponse<Contact?>(null, 500, "Não foi possível Deletar o Contato!");
            }
        }


        public PagedContactResponse<List<Contact>?> GetAll(GetAllContactRequest request)
        {
            try
            {
                var query = _contact.GetAll(request.Name, request.Email, request.Ddd, request.PhoneNumber);

                var contacts = query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var count = query.Count();

                return new PagedContactResponse<List<Contact>?>(contacts, count, request.PageNumber, request.PageSize);
            }
            catch
            {
                return new PagedContactResponse<List<Contact>?>(null, 500, "Não foi possível consultar os Contatos!");
            }
        }

        public PagedContactResponse<List<Contact>?> GetAllByDdd(GetAllContactRequest request)
        {
            try
            {
                var query = _contact.GetAll("", "", request.Ddd, "");

                var contacts = query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var count = query.Count();

                return new PagedContactResponse<List<Contact>?>(contacts, count, request.PageNumber, request.PageSize);
            }
            catch
            {
                return new PagedContactResponse<List<Contact>?>(null, 500, "Não foi possível consultar os Contatos!");
            }
        }

    }
}
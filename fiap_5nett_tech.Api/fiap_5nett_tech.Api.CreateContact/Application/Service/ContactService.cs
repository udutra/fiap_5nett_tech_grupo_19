using fiap_5nett_tech.Api.CreateContact.Application.DataTransfer.Request;
using fiap_5nett_tech.Api.CreateContact.Application.DataTransfer.Response;
using fiap_5nett_tech.Api.CreateContact.Application.Interface;
using fiap_5nett_tech.Api.CreateContact.Domain.Entities;
using fiap_5nett_tech.Api.CreateContact.Domain.Repositories;

namespace fiap_5nett_tech.Api.CreateContact.Application.Service;

public class ContactService(IContactRepository contact, IRegionRepository region) : IContactInterface
    {
        public ContactResponse<Contact?> Create(ContactRequest request)
        {
            try
            {
                var region1 = region.GetOne(request.Ddd);

                if (region1 == null)
                {
                    return new ContactResponse<Contact?>(null, 400, "Região não encontrada!");
                }

                if (contact.GetOne(request.Ddd, request.PhoneNumber) != null)
                {
                    return new ContactResponse<Contact?>(null, 400, "Telefone já Cadastrado!");
                }

                if (request.PhoneNumber.Length != 9)
                {
                    return new ContactResponse<Contact?>(null, 400, "Quantidade de caracteres de telefone invalido!");
                }

                Contact contact1 = new(request.Name, request.Email, request.PhoneNumber, region1);
                contact.Create(contact1);
                return new ContactResponse<Contact?>(contact1, 200, "Contato criado com sucesso!");
            }
            catch
            {
                return new ContactResponse<Contact?>(null, 500, "Não foi possível criar o contato!");
            }
        }

        public ContactResponse<Contact?> Update(ContactRequest contactRequest)
        {
            try
            {
                var contact1 = contact.GetOne(contactRequest.Ddd, contactRequest.PhoneNumber);

                if (contact1 is null)
                    return new ContactResponse<Contact?>(null, 404, "Contato não encontrado!");

                if (!string.IsNullOrEmpty(contactRequest.Name))
                    contact1.Name = contactRequest.Name;

                if (!string.IsNullOrEmpty(contactRequest.Email))
                    contact1.Email = contactRequest.Email;

                contact.Update(contact1);

                return new ContactResponse<Contact?>(contact1, message: "Contato atualizado com sucesso!");
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
                var contact1 = contact.GetOne(id);

                return contact1 is null
                    ? new ContactResponse<Contact?>(null, 404, "Contato não encontrado!")
                    : new ContactResponse<Contact?>(contact1);
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
                var contact1 = contact.GetOne(ddd, telefone);

                return contact1 is null
                    ? new ContactResponse<Contact?>(null, 404, "Contato não encontrado!")
                    : new ContactResponse<Contact?>(contact1);
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
                var contact1 = contact.Delete(ddd, telefone);

                return contact1 is null
                    ? new ContactResponse<Contact?>(null, 404, "Contato não encontrado!")
                    : new ContactResponse<Contact?>(contact1, 200, "Contato excluído com sucesso!");
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
                var query = contact.GetAll(request.Name, request.Email, request.Ddd, request.PhoneNumber);

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
                var query = contact.GetAll("", "", request.Ddd, "");

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
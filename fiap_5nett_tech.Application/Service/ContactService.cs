using System.ComponentModel.DataAnnotations;
using fiap_5nett_tech.Application.DataTransfer.Request;
using fiap_5nett_tech.Application.Interface;
using fiap_5nett_tech.Domain.Entities;
using fiap_5nett_tech.Application.DataTransfer.Response;
using fiap_5nett_tech.Domain.Repositories;


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

        public ContactResponse<Contact?> Create(ContactRequest request)
        {
            try
            {
                var region = _region.GetOne(request.Ddd);

                if (region == null)
                {
                    return new ContactResponse<Contact?>(null, 400, "Região não encontrada!");
                }

                if (_contact.GetOne(request.Ddd, request.PhoneNumber) != null)
                {
                    return new ContactResponse<Contact?>(null, 400, "Telefone já Cadastrado!");
                }

                if (request.PhoneNumber.Length != 9)
                {
                    return new ContactResponse<Contact?>(null, 400, "Quantidade de caracteres de telefone invalido!");
                }

                Contact contact = new(request.Name, request.Email, request.PhoneNumber, region);
                _contact.Create(contact);
                return new ContactResponse<Contact?>(contact, 200, "Contato criado com sucesso!");
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
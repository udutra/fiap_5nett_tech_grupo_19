using fiap_5nett_tech.Api.CreateContact.Application.DataTransfer.Request;
using fiap_5nett_tech.Api.CreateContact.Application.DataTransfer.Response;
using fiap_5nett_tech.Api.CreateContact.Domain.Entities;

namespace fiap_5nett_tech.Api.CreateContact.Application.Interface;

public interface IContactInterface
{
    ContactResponse<Contact?> Create(ContactRequest request);
    ContactResponse<Contact?> GetOne(Guid id);
    ContactResponse<Contact?> GetOne(int ddd, string telefone);
    ContactResponse<Contact?> Delete(int ddd, string telefone);
    ContactResponse<Contact?> Update(ContactRequest request);
    PagedContactResponse<List<Contact>?> GetAll(GetAllContactRequest request);
    PagedContactResponse<List<Contact>?> GetAllByDdd(GetAllContactRequest request);
}
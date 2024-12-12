﻿using fiap_5nett_tech.Application.DataTransfer.Request;
using fiap_5nett_tech.Application.DataTransfer.Response;
using fiap_5nett_tech.Domain.Entities;

namespace fiap_5nett_tech.api.Interfaces
{
    public interface IContactInterface
    {
        Task<ContactResponse<Contact?>> Create();
        ContactResponse<Contact?> GetOne(Guid id);
        ContactResponse<Contact?> GetOne(int ddd, string telefone);
        ContactResponse<Contact?> Delete(int ddd, string telefone);
        ContactResponse<Contact?> Update(ContactRequest request);
        PagedContactResponse<List<Contact>?> GetAll(GetAllContactRequest request);
        PagedContactResponse<List<Contact>?> GetAllByDdd(GetAllContactRequest request);
    }
}
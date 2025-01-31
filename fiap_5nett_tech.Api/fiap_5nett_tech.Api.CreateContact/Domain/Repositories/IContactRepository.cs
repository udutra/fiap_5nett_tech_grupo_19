using fiap_5nett_tech.Api.CreateContact.Domain.Entities;

namespace fiap_5nett_tech.Api.CreateContact.Domain.Repositories;

public interface IContactRepository
{
    void Create(Contact contact);
    void Update(Contact contact);
    Contact? Delete(int ddd, string telefone);
    IQueryable<Contact> GetAll(string nome, string email, int ddd, string telefone);
    Contact? GetOne(Guid id);
    Contact? GetOne(int ddd, string telefone);
}
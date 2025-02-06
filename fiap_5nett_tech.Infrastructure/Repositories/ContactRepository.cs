using fiap_5nett_tech.Domain.Entities;
using fiap_5nett_tech.Domain.Repositories;
using System.Data.Common;
using fiap_5nett_tech.Infrastructure.Data;


namespace fiap_5nett_tech.Infrastructure.Repositories
{
    public class ContactRepository(AppDbContext context) : IContactRepository {
        public void Create(Contact contact) {
            try {
                context.Contacts.Add(contact);
                context.SaveChanges();
            }
            catch (DbException ex) {
                Console.WriteLine(ex.Message);
            }
        }

        public void Update(Contact contact)
        {
            try
            {
                context.Contacts.Update(contact);
                context.SaveChanges();
            }
            catch (DbException ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public Contact? Delete(int ddd, string telefone)
        {
            try
            {
                var contact = context.Contacts.FirstOrDefault(x => x.Region.Ddd == ddd && x.Phone == telefone);

                if (contact is null)
                {
                    return null;

                }
                context.Contacts.Remove(contact);
                context.SaveChanges();
                return contact;

            }
            catch (DbException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public IQueryable<Contact> GetAll(string? nome, string? email, int ddd, string? telefone)
        {
            try
            {
                var contacts = context.Contacts.AsQueryable();

                if (!string.IsNullOrWhiteSpace(nome))
                {
                    contacts = contacts.Where(x => x.Name.Contains(nome));
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    contacts = contacts.Where(x => x.Email.Contains(email));
                }

                if (ddd > 0)
                {
                    contacts = contacts.Where(x => x.Region.Ddd == ddd);
                }

                if (!string.IsNullOrWhiteSpace(telefone))
                {
                    contacts = contacts.Where(x => x.Phone.Contains(telefone));
                }

                return contacts;
            }
            catch (DbException ex)
            {
                Console.WriteLine(ex.Message);
                return Enumerable.Empty<Contact>().AsQueryable();
            }
        }

        public Contact? GetOne(Guid id)
        {
            try
            {
                return context
                    .Contacts
                    .FirstOrDefault(x => x.Id == id);
            }
            catch (DbException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public Contact? GetOne(int ddd, string telefone)
        {
            try
            {
                return context
                    .Contacts
                    .FirstOrDefault(x => x.Region.Ddd == ddd && x.Phone == telefone);
            }
            catch (DbException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}

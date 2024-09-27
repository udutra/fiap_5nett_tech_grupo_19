using fiap_5nett_tech.Domain.Entities;
using fiap_5nett_tech.Domain.Repositories;
using System.Data.Common;
using fiap_5nett_tech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace fiap_5nett_tech.Infrastructure.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly AppDbContext _context;

        public ContactRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Create(Contact contact)
        {
            try
            {
                _context.Contacts.Add(contact);
                _context.SaveChanges();
            }
            catch (DbException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Update(Contact contact)
        {
            try
            {
                _context.Contacts.Update(contact);
                _context.SaveChanges();
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
                var contact = _context.Contacts.FirstOrDefault(x => x.Region.Ddd == ddd && x.Phone == telefone);

                if (contact is null)
                {
                    return null;

                }
                _context.Contacts.Remove(contact);
                _context.SaveChanges();
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
                var contacts = _context.Contacts.AsQueryable();

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
                return _context
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
                return _context
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

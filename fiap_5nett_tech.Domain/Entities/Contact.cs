using System.Text.Json.Serialization;

namespace fiap_5nett_tech.Domain.Entities;

public class Contact
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public string Email { get; set; }
    
    public string Phone { get; set; }
    
    [JsonIgnore]
    public Region Region { get; set; }
    
    public int Ddd { get; set; }

    public Contact(string name, string email, string phone, Region region)
    {
        Name = name;
        Email = email;
        Phone = phone;
        Region = region;
    }

    public Contact()
    {
    }
}
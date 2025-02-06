namespace fiap_5nett_tech.Domain.Entities;

public class Region
{
    public int Ddd { get; set; }
    public string Name { get; set; }

    public virtual List<Contact> Contacts { get; set; }

    public Region()
    {
    }

    public Region(int ddd, string name)
    {
        Ddd = ddd;
        Name = name;
        Contacts = [];
    }
}
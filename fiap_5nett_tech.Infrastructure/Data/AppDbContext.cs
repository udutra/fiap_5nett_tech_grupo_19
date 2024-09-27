using fiap_5nett_tech.Domain.Entities;
using fiap_5nett_tech.Infrastructure.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace fiap_5nett_tech.Infrastructure.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Region> Regions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ContactMapping());
        modelBuilder.ApplyConfiguration(new RegionMapping());

        modelBuilder.Entity<Contact>()
            .HasOne<Region>(c => c.Region)
            .WithMany(r => r.Contacts)
            .HasForeignKey(c => c.Ddd)
            .HasConstraintName("FK_CONTACT_REGION");
        
        base.OnModelCreating(modelBuilder);

        

        modelBuilder.Entity<Region>().HasData(
        [
            new Region(11, "São Paulo"),
            new Region(12, "São Paulo"),
            new Region(13, "São Paulo"),
            new Region(14, "São Paulo"),
            new Region(15, "São Paulo"),
            new Region(16, "São Paulo"),
            new Region(17, "São Paulo"),
            new Region(18, "São Paulo"),
            new Region(19, "São Paulo"),
            new Region(21, "Rio de Janeiro"),
            new Region(22, "Rio de Janeiro"),
            new Region(24, "Rio de Janeiro"),
            new Region(27, "Espírito Santo"),
            new Region(28, "Espírito Santo"),
            new Region(31, "Minas Gerais"),
            new Region(32, "Minas Gerais"),
            new Region(33, "Minas Gerais"),
            new Region(34, "Minas Gerais"),
            new Region(35, "Minas Gerais"),
            new Region(37, "Minas Gerais"),
            new Region(38, "Minas Gerais"),
            new Region(41, "Paraná"),
            new Region(42, "Paraná"),
            new Region(43, "Paraná"),
            new Region(44, "Paraná"),
            new Region(45, "Paraná"),
            new Region(46, "Paraná"),
            new Region(47, "Santa Catarina"),
            new Region(48, "Santa Catarina"),
            new Region(49, "Santa Catarina"),
            new Region(51, "Rio Grande do Sul"),
            new Region(53, "Rio Grande do Sul"),
            new Region(54, "Rio Grande do Sul"),
            new Region(55, "Rio Grande do Sul"),
            new Region(61, "Distrito Federal"),
            new Region(62, "Goiás"),
            new Region(63, "Tocantins"),
            new Region(64, "Goiás"),
            new Region(65, "Mato Grosso"),
            new Region(66, "Mato Grosso"),
            new Region(67, "Mato Grosso do Sul"),
            new Region(68, "Acre"),
            new Region(69, "Rondônia"),
            new Region(71, "Bahia"),
            new Region(73, "Bahia"),
            new Region(74, "Bahia"),
            new Region(75, "Bahia"),
            new Region(77, "Bahia"),
            new Region(79, "Sergipe"),
            new Region(81, "Pernambuco"),
            new Region(82, "Alagoas"),
            new Region(83, "Paraíba"),
            new Region(84, "Rio Grande do Norte"),
            new Region(85, "Ceará"),
            new Region(86, "Piauí"),
            new Region(87, "Pernambuco"),
            new Region(88, "Ceará"),
            new Region(89, "Piauí"),
            new Region(91, "Pará"),
            new Region(92, "Amazonas"),
            new Region(93, "Pará"),
            new Region(94, "Pará"),
            new Region(95, "Roraima"),
            new Region(96, "Amapá"),
            new Region(97, "Amazonas"),
            new Region(98, "Maranhão"),
            new Region(99, "Maranhão")
        ]);
    }   
}
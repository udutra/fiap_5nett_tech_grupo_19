using fiap_5nett_tech.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace fiap_5nett_tech.Infrastructure.Data.Mappings;

public class RegionMapping : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable("Region");
        builder.HasKey(x => x.Ddd);

        builder.Property(x => x.Ddd)
            .ValueGeneratedNever();
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasColumnType("NVARCHAR")
            .HasMaxLength(80);
    }
}
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class DestinationConfiguration : IEntityTypeConfiguration<Destination>
{
    public void Configure(EntityTypeBuilder<Destination> builder)
    {
        builder.ToTable("Destinations");
        // Primary Key
        builder.HasKey(d => d.Id);
        // Configure properties
        builder.Property(d => d.Address).IsRequired();
        builder.Property(d => d.Name).IsRequired();
        
        //Soft Delete Query Filter
        builder.HasQueryFilter(q => !q.IsDeleted);
        builder.HasIndex(i => i.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}
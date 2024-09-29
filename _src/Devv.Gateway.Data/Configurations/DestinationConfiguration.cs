using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

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
    }
}
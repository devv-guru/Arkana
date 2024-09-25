using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class DestinationConfigConfiguration : IEntityTypeConfiguration<DestinationConfig>
{
    public void Configure(EntityTypeBuilder<DestinationConfig> builder)
    {
        // Primary Key
        builder.HasKey(d => d.Id);
        // Configure properties
        builder.Property(d => d.Address).IsRequired();
        builder.Property(d => d.Name).IsRequired();
    }
}
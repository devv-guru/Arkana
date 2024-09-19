using Devv.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Data.Configurations;

public class DestinationConfigConfiguration : IEntityTypeConfiguration<DestinationConfig>
{
    public void Configure(EntityTypeBuilder<DestinationConfig> builder)
    {
        // Configure properties
        builder.Property(d => d.Address).IsRequired();
        builder.Property(d => d.DestinationId).IsRequired();
    }
}
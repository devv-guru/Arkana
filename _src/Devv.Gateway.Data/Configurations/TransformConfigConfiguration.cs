using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class TransformConfigConfiguration : IEntityTypeConfiguration<TransformConfig>
{
    public void Configure(EntityTypeBuilder<TransformConfig> builder)
    {
        // Configure properties
        builder.Property(t => t.RequestHeader).IsRequired();
        builder.Property(t => t.Set).IsRequired();
    }
}
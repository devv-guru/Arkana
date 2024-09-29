using Devv.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.Gateway.Data.Configurations;

public class TransformConfiguration : IEntityTypeConfiguration<Transform>
{
    public void Configure(EntityTypeBuilder<Transform> builder)
    {
        builder.ToTable("Transforms");
        // Primary Key
        builder.HasKey(t => t.Id);

        // Configure properties
        builder.Property(t => t.RequestHeader).IsRequired();
        builder.Property(t => t.Set).IsRequired();
    }
}
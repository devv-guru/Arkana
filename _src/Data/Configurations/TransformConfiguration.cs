using Data.Entities.Proxy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

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

        //Soft Delete Query Filter
        builder.HasQueryFilter(q => !q.IsDeleted);
        builder.HasIndex(i => i.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class HttpRequestSettingsConfiguration : IEntityTypeConfiguration<HttpRequestSettings>
{
    public void Configure(EntityTypeBuilder<HttpRequestSettings> builder)
    {
        builder.ToTable("HttpRequestSettings");
        // Primary Key
        builder.HasKey(h => h.Id);
        // Configure properties
        builder.Property(h => h.Version).IsRequired();
        builder.Property(h => h.ActivityTimeout).IsRequired();
        
        //Soft Delete Query Filter
        builder.HasQueryFilter(q => !q.IsDeleted);
        builder.HasIndex(i => i.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}
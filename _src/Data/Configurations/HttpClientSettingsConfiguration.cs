using Data.Entities.Proxy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class HttpClientSettingsConfiguration : IEntityTypeConfiguration<HttpClientSettings>
{
    public void Configure(EntityTypeBuilder<HttpClientSettings> builder)
    {
        builder.ToTable("HttpClientSettings");
        // Primary Key
        builder.HasKey(h => h.Id);

        // Configure properties
        builder.Property(h => h.SslProtocols).IsRequired();
        builder.Property(h => h.MaxConnectionsPerServer).IsRequired();

        //Soft Delete Query Filter
        builder.HasQueryFilter(q => !q.IsDeleted);
        builder.HasIndex(i => i.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}
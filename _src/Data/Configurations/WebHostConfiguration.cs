using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class WebHostConfiguration : IEntityTypeConfiguration<WebHost>
{
    public void Configure(EntityTypeBuilder<WebHost> builder)
    {
        builder.ToTable("WebHosts");
        // Primary Key
        builder.HasKey(h => h.Id);

        // Configure relationships
        // Certificate relationship removed - Azure handles SSL termination
        
        builder.HasOne(h => h.Cluster)
            .WithOne(o => o.Host)
            .HasForeignKey<Cluster>(f => f.HostId);

        builder.HasMany(m => m.Routes)
            .WithOne(o => o.Host)
            .HasForeignKey(f => f.HostId);

        // Configure properties
        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(h => h.HostName)
            .IsRequired()
            .HasMaxLength(500);

        //Soft Delete Query Filter
        builder.HasQueryFilter(q => !q.IsDeleted);
        builder.HasIndex(i => i.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}
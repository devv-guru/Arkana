using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.ToTable("Routes");
        // Primary Key
        builder.HasKey(r => r.Id);

        // Configure relationships
        builder.HasOne(r => r.Match)
            .WithOne(m => m.Route)
            .HasForeignKey<Match>(m => m.RouteConfigId);

        builder.HasOne(c => c.Metadata)
            .WithOne(m => m.Route)
            .HasForeignKey<Metadata>(m => m.ClusterConfigId);

        // Configure properties
        builder.Property(r => r.ClusterId)
            .IsRequired();
        builder.Property(r => r.AuthorizationPolicy)
            .HasMaxLength(100);
        builder.Property(r => r.CorsPolicy)
            .HasMaxLength(100);
        
        //Soft Delete Query Filter
        builder.HasQueryFilter(q => !q.IsDeleted);
        builder.HasIndex(i => i.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}
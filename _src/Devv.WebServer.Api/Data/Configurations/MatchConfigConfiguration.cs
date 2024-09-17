using Devv.WebServer.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.WebServer.Api.Data.Configurations;

public class MatchConfigConfiguration : IEntityTypeConfiguration<MatchConfig>
{
    public void Configure(EntityTypeBuilder<MatchConfig> builder)
    {
        builder.HasMany(m => m.Headers)
            .WithOne(h => h.MatchConfig)
            .HasForeignKey(h => h.MatchConfigId);

        builder.HasMany(m => m.QueryParameters)
            .WithOne(q => q.MatchConfig)
            .HasForeignKey(q => q.MatchConfigId);

        builder.Property(m => m.Path).IsRequired();
    }
}
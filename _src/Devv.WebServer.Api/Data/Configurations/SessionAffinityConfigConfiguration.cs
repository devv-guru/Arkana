using Devv.WebServer.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devv.WebServer.Api.Data.Configurations;

public class SessionAffinityConfigConfiguration : IEntityTypeConfiguration<SessionAffinityConfig>
{
    public void Configure(EntityTypeBuilder<SessionAffinityConfig> builder)
    {
        // Configure properties
        builder.Property(s => s.Policy).IsRequired();
        builder.Property(s => s.FailurePolicy).IsRequired();
    }
}
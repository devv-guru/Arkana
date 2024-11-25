using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");
        
        builder.HasKey(m => m.Id);

        builder.HasMany(m => m.Headers)
            .WithOne(h => h.MatchConfig)
            .HasForeignKey(h => h.MatchConfigId);

        builder.HasMany(m => m.QueryParameters)
            .WithOne(q => q.Match)
            .HasForeignKey(q => q.MatchConfigId);

        builder.Property(m => m.Path).IsRequired();
        
        //Soft Delete Query Filter
        builder.HasQueryFilter(q => !q.IsDeleted);
        builder.HasIndex(i => i.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}
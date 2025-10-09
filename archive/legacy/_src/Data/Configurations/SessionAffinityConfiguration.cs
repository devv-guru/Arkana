using Data.Entities;
using Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class SessionAffinityConfiguration : IEntityTypeConfiguration<SessionAffinity>
{
    public void Configure(EntityTypeBuilder<SessionAffinity> builder)
    {
        builder.ToTable("SessionAffinity");
        // Primary Key
        builder.HasKey(s => s.Id);

        // Value converter
        var converter = new DictionaryToStringConverter();

        // Value comparer
        var dictionaryComparer = new ValueComparer<Dictionary<string, string>>(
            (d1, d2) => d1.SequenceEqual(d2),
            d => d.Aggregate(0, (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value.GetHashCode())),
            d => new Dictionary<string, string>(d)
        );

        // Configure properties
        builder.Property(s => s.Policy).IsRequired();
        builder.Property(s => s.FailurePolicy).IsRequired();
        builder.Property(s => s.Settings)
            .HasConversion(converter)
            .Metadata.SetValueComparer(dictionaryComparer);
        
        //Soft Delete Query Filter
        builder.HasQueryFilter(q => !q.IsDeleted);
        builder.HasIndex(i => i.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}
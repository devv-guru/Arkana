using Data.Entities.Proxy;
using Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class HeaderMatchConfiguration : IEntityTypeConfiguration<HeaderMatch>
{
    public void Configure(EntityTypeBuilder<HeaderMatch> builder)
    {
        builder.ToTable("HeaderMatches");

        // Primary Key
        builder.HasKey(h => h.Id);

        // Value converter
        var converter = new ListToStringConverter();

        // Value comparer
        var stringCollectionComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1.SequenceEqual(c2), // Comparison logic
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), // Hash code generation
            c => c.ToList()); // Cloning logic, deep copy

        // Configure properties
        builder.Property(h => h.Name).IsRequired();
        builder.Property(h => h.Mode).IsRequired();
        builder.Property(h => h.Values)
            .HasConversion(converter)
            .Metadata.SetValueComparer(stringCollectionComparer);

        //Soft Delete Query Filter
        builder.HasQueryFilter(q => !q.IsDeleted);
        builder.HasIndex(i => i.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}
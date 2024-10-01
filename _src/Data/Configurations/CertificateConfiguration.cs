using Data.Entities;
using Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.ToTable("Certificates");
        // Primary Key
        builder.HasKey(c => c.Id);
        // Converters
        var converter = new ArrayToStringConverter();

        // Value comparer
        var stringCollectionComparer = new ValueComparer<string[]>(
            (c1, c2) => c1.SequenceEqual(c2), // Comparison logic
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), // Hash code generation
            c => c.ToArray()); // Cloning logic, deep copy

        // Configure properties
        builder.Property(c => c.Name).HasMaxLength(500);
        builder.Property(c => c.CertificateSource).IsRequired();
        builder.Property(c => c.KeyVaultUri).HasMaxLength(1000);
        builder.Property(c => c.KeyVaultName).HasMaxLength(500);
        builder.Property(c => c.KeyVaultCertificateName).HasMaxLength(500);
        builder.Property(c => c.KeyVaultCertificatePasswordName).HasMaxLength(500);
        builder.Property(c => c.AwsRegion).HasMaxLength(500);
        builder.Property(c => c.AwsCertificateName).HasMaxLength(500);
        builder.Property(c => c.AwsCertificatePasswordName).HasMaxLength(500);
        builder.Property(c => c.SubjectAlternativeNames).HasMaxLength(1000)
            .HasConversion(converter)
            .Metadata.SetValueComparer(stringCollectionComparer);
        
        //Soft Delete Query Filter
        builder.HasQueryFilter(q => !q.IsDeleted);
        builder.HasIndex(i => i.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}
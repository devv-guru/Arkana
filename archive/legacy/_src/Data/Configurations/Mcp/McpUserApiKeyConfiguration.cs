using Data.Entities.Mcp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations.Mcp;

public class McpUserApiKeyConfiguration : IEntityTypeConfiguration<McpUserApiKey>
{
    public void Configure(EntityTypeBuilder<McpUserApiKey> builder)
    {
        builder.ToTable("McpUserApiKeys");
        
        // Primary Key
        builder.HasKey(u => u.Id);
        
        // Properties
        builder.Property(u => u.UserId)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("OIDC subject/user identifier");
            
        builder.Property(u => u.UserEmail)
            .IsRequired()
            .HasMaxLength(320)
            .HasComment("User email address");
            
        builder.Property(u => u.ApiKey)
            .IsRequired()
            .HasMaxLength(2000)
            .HasComment("Encrypted user-specific API key");
            
        builder.Property(u => u.IsEnabled)
            .HasDefaultValue(true)
            .HasComment("Whether the API key is active");
            
        builder.Property(u => u.ExpiresAt)
            .HasComment("Optional expiration date for the API key");
            
        builder.Property(u => u.LastUsedAt)
            .HasComment("Timestamp of last API key usage");

        // Indexes
        builder.HasIndex(u => new { u.McpBackendAuthId, u.UserId })
            .IsUnique()
            .HasFilter("IsDeleted = 0")
            .HasDatabaseName("IX_McpUserApiKeys_BackendAuthId_UserId");
            
        builder.HasIndex(u => u.UserId)
            .HasDatabaseName("IX_McpUserApiKeys_UserId");
            
        builder.HasIndex(u => u.UserEmail)
            .HasDatabaseName("IX_McpUserApiKeys_UserEmail");
            
        builder.HasIndex(u => u.IsEnabled);
        builder.HasIndex(u => u.ExpiresAt);
        builder.HasIndex(u => u.LastUsedAt);

        // Soft Delete Query Filter
        builder.HasQueryFilter(u => !u.IsDeleted);
        builder.HasIndex(u => u.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}
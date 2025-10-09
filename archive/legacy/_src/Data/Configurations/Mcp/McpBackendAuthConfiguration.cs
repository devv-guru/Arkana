using Data.Entities.Mcp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations.Mcp;

public class McpBackendAuthConfiguration : IEntityTypeConfiguration<McpBackendAuth>
{
    public void Configure(EntityTypeBuilder<McpBackendAuth> builder)
    {
        builder.ToTable("McpBackendAuths");
        
        // Primary Key
        builder.HasKey(b => b.Id);
        
        // Properties
        builder.Property(b => b.AuthType)
            .HasComment("Authentication type: None, OAuth2, ApiKey, Bearer");
            
        // OAuth2 Properties
        builder.Property(b => b.ClientId)
            .HasMaxLength(500)
            .HasComment("OAuth2 client ID");
            
        builder.Property(b => b.ClientSecret)
            .HasMaxLength(2000)
            .HasComment("Encrypted OAuth2 client secret");
            
        builder.Property(b => b.TokenEndpoint)
            .HasMaxLength(2000)
            .HasComment("OAuth2 token endpoint URL");
            
        builder.Property(b => b.AuthorizationEndpoint)
            .HasMaxLength(2000)
            .HasComment("OAuth2 authorization endpoint URL");
            
        builder.Property(b => b.Scope)
            .HasMaxLength(1000)
            .HasComment("OAuth2 requested scopes");
            
        builder.Property(b => b.RedirectUri)
            .HasMaxLength(2000)
            .HasComment("OAuth2 redirect URI");
            
        // API Key Properties
        builder.Property(b => b.ApiKey)
            .HasMaxLength(2000)
            .HasComment("Encrypted global API key");
            
        builder.Property(b => b.ApiKeyHeader)
            .HasMaxLength(100)
            .HasDefaultValue("Authorization")
            .HasComment("Header name for API key");
            
        builder.Property(b => b.ApiKeyPrefix)
            .HasMaxLength(50)
            .HasDefaultValue("Bearer")
            .HasComment("Prefix for API key value");
            
        builder.Property(b => b.AllowPerUserApiKeys)
            .HasDefaultValue(false)
            .HasComment("Whether to allow per-user API keys");
            
        // Additional Configuration
        builder.Property(b => b.CustomHeaders)
            .HasMaxLength(4000)
            .HasComment("JSON serialized custom headers");
            
        builder.Property(b => b.TokenCacheTtlSeconds)
            .HasDefaultValue(3600)
            .HasComment("Token cache TTL in seconds");
            
        builder.Property(b => b.EnableTokenRefresh)
            .HasDefaultValue(true)
            .HasComment("Whether to enable automatic token refresh");

        // Indexes
        builder.HasIndex(b => b.McpServerId)
            .IsUnique()
            .HasFilter("IsDeleted = 0");
            
        builder.HasIndex(b => b.AuthType);

        // Relationships
        builder.HasMany(b => b.UserApiKeys)
            .WithOne(u => u.McpBackendAuth)
            .HasForeignKey(u => u.McpBackendAuthId)
            .OnDelete(DeleteBehavior.Cascade);

        // Soft Delete Query Filter
        builder.HasQueryFilter(b => !b.IsDeleted);
        builder.HasIndex(b => b.IsDeleted)
            .HasFilter("IsDeleted = 0");
    }
}
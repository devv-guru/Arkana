using Gateway.Models;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Data;

public class GatewayDbContext : DbContext
{
    public GatewayDbContext(DbContextOptions<GatewayDbContext> options) : base(options)
    {
    }

    public DbSet<McpServer> McpServers => Set<McpServer>();
    public DbSet<McpUserAccess> McpUserAccess => Set<McpUserAccess>();
    public DbSet<McpAuditLog> McpAuditLogs => Set<McpAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // McpServer configuration
        modelBuilder.Entity<McpServer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // McpUserAccess configuration
        modelBuilder.Entity<McpUserAccess>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.McpServerId, e.UserId }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.McpServer)
                  .WithMany(s => s.UserAccess)
                  .HasForeignKey(e => e.McpServerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // McpAuditLog configuration
        modelBuilder.Entity<McpAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.McpServer)
                  .WithMany()
                  .HasForeignKey(e => e.McpServerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Seed some test data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Sample MCP server for testing
        modelBuilder.Entity<McpServer>().HasData(
            new McpServer
            {
                Id = 1,
                Name = "calculator",
                Description = "Basic arithmetic calculator tool",
                Endpoint = "https://localhost:7001",
                Protocol = McpProtocolType.Http,
                AuthType = McpAuthType.Bearer,
                IsEnabled = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
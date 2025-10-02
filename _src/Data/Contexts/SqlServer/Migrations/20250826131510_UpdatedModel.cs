using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Contexts.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwsCertificateName",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "AwsCertificatePasswordName",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "AwsRegion",
                table: "Certificates");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Clusters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubjectAlternativeNames",
                table: "Certificates",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "Subject Alternative Names for self-signed certificates",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Certificates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                comment: "Certificate display name",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KeyVaultUri",
                table: "Certificates",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "Azure Key Vault URI",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KeyVaultName",
                table: "Certificates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                comment: "Azure Key Vault name",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KeyVaultCertificatePasswordName",
                table: "Certificates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                comment: "Certificate password secret name in Azure Key Vault",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KeyVaultCertificateName",
                table: "Certificates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                comment: "Certificate name in Azure Key Vault",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CertificateSource",
                table: "Certificates",
                type: "nvarchar(max)",
                nullable: false,
                comment: "Certificate source type: Local, KeyVault, InMemory, SelfSigned",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "FilePassword",
                table: "Certificates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                comment: "Local certificate file password");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Certificates",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "Local certificate file path");

            migrationBuilder.CreateTable(
                name: "McpServers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Unique name for the MCP server"),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false, comment: "Description of the MCP server functionality"),
                    Endpoint = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false, comment: "WebSocket or SSE endpoint URL"),
                    ProtocolType = table.Column<int>(type: "int", nullable: false, comment: "Protocol type: WebSocket, SSE, or HTTP"),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "Whether the MCP server is enabled"),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "Priority for routing (lower = higher priority)"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpServers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequestMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RouteId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClusterId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DestinationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Host = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    ElapsedMilliseconds = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClientIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestSize = table.Column<long>(type: "bigint", nullable: false),
                    ResponseSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CpuUsagePercent = table.Column<double>(type: "float", nullable: false),
                    MemoryUsageMB = table.Column<double>(type: "float", nullable: false),
                    TotalMemoryMB = table.Column<double>(type: "float", nullable: false),
                    ThreadCount = table.Column<int>(type: "int", nullable: false),
                    HandleCount = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NetworkInboundKbps = table.Column<double>(type: "float", nullable: false),
                    NetworkOutboundKbps = table.Column<double>(type: "float", nullable: false),
                    DiskReadKbps = table.Column<double>(type: "float", nullable: false),
                    DiskWriteKbps = table.Column<double>(type: "float", nullable: false),
                    ActiveConnections = table.Column<int>(type: "int", nullable: false),
                    HostName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "McpAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    McpServerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "OIDC subject/user identifier"),
                    UserEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false, comment: "User email address"),
                    EventType = table.Column<int>(type: "int", nullable: false, comment: "Type of audit event"),
                    EventDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false, comment: "Description of the event"),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true, comment: "Client IP address (supports IPv6)"),
                    UserAgent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, comment: "Client user agent string"),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "Session identifier"),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true, comment: "JSON serialized additional event data"),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "Whether the event was successful"),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true, comment: "Error message if event failed"),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: true, comment: "Duration of the operation"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McpAuditLogs_McpServers_McpServerId",
                        column: x => x.McpServerId,
                        principalTable: "McpServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "McpBackendAuths",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    McpServerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthType = table.Column<int>(type: "int", nullable: false, comment: "Authentication type: None, OAuth2, ApiKey, Bearer"),
                    ClientId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "OAuth2 client ID"),
                    ClientSecret = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true, comment: "Encrypted OAuth2 client secret"),
                    TokenEndpoint = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true, comment: "OAuth2 token endpoint URL"),
                    AuthorizationEndpoint = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true, comment: "OAuth2 authorization endpoint URL"),
                    Scope = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, comment: "OAuth2 requested scopes"),
                    RedirectUri = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true, comment: "OAuth2 redirect URI"),
                    ApiKey = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true, comment: "Encrypted global API key"),
                    ApiKeyHeader = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, defaultValue: "Authorization", comment: "Header name for API key"),
                    ApiKeyPrefix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Bearer", comment: "Prefix for API key value"),
                    AllowPerUserApiKeys = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Whether to allow per-user API keys"),
                    CustomHeaders = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true, comment: "JSON serialized custom headers"),
                    TokenCacheTtlSeconds = table.Column<int>(type: "int", nullable: false, defaultValue: 3600, comment: "Token cache TTL in seconds"),
                    EnableTokenRefresh = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "Whether to enable automatic token refresh"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpBackendAuths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McpBackendAuths_McpServers_McpServerId",
                        column: x => x.McpServerId,
                        principalTable: "McpServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "McpRoleAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    McpServerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "OIDC role/group name"),
                    RoleDisplayName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false, comment: "Human-readable role name"),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "Whether the assignment is active"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Optional expiration date for the assignment"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpRoleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McpRoleAssignments_McpServers_McpServerId",
                        column: x => x.McpServerId,
                        principalTable: "McpServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "McpUserAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    McpServerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "OIDC subject/user identifier"),
                    UserEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false, comment: "User email address"),
                    UserDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "User display name"),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "Whether the assignment is active"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Optional expiration date for the assignment"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpUserAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McpUserAssignments_McpServers_McpServerId",
                        column: x => x.McpServerId,
                        principalTable: "McpServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "McpUserApiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    McpBackendAuthId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "OIDC subject/user identifier"),
                    UserEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false, comment: "User email address"),
                    ApiKey = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false, comment: "Encrypted user-specific API key"),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "Whether the API key is active"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Optional expiration date for the API key"),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Timestamp of last API key usage"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpUserApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McpUserApiKeys_McpBackendAuths_McpBackendAuthId",
                        column: x => x.McpBackendAuthId,
                        principalTable: "McpBackendAuths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_McpAuditLogs_CreatedAt",
                table: "McpAuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_McpAuditLogs_EventType",
                table: "McpAuditLogs",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_McpAuditLogs_EventType_CreatedAt",
                table: "McpAuditLogs",
                columns: new[] { "EventType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_McpAuditLogs_IsSuccess",
                table: "McpAuditLogs",
                column: "IsSuccess");

            migrationBuilder.CreateIndex(
                name: "IX_McpAuditLogs_McpServerId",
                table: "McpAuditLogs",
                column: "McpServerId");

            migrationBuilder.CreateIndex(
                name: "IX_McpAuditLogs_ServerId_CreatedAt",
                table: "McpAuditLogs",
                columns: new[] { "McpServerId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_McpAuditLogs_UserEmail",
                table: "McpAuditLogs",
                column: "UserEmail");

            migrationBuilder.CreateIndex(
                name: "IX_McpAuditLogs_UserId",
                table: "McpAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_McpAuditLogs_UserId_CreatedAt",
                table: "McpAuditLogs",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_McpBackendAuths_AuthType",
                table: "McpBackendAuths",
                column: "AuthType");

            migrationBuilder.CreateIndex(
                name: "IX_McpBackendAuths_IsDeleted",
                table: "McpBackendAuths",
                column: "IsDeleted",
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_McpBackendAuths_McpServerId",
                table: "McpBackendAuths",
                column: "McpServerId",
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_McpRoleAssignments_ExpiresAt",
                table: "McpRoleAssignments",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_McpRoleAssignments_IsDeleted",
                table: "McpRoleAssignments",
                column: "IsDeleted",
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_McpRoleAssignments_IsEnabled",
                table: "McpRoleAssignments",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_McpRoleAssignments_RoleName",
                table: "McpRoleAssignments",
                column: "RoleName");

            migrationBuilder.CreateIndex(
                name: "IX_McpRoleAssignments_ServerId_RoleName",
                table: "McpRoleAssignments",
                columns: new[] { "McpServerId", "RoleName" },
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_McpServers_IsDeleted",
                table: "McpServers",
                column: "IsDeleted",
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_McpServers_IsEnabled",
                table: "McpServers",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_McpServers_Name",
                table: "McpServers",
                column: "Name",
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_McpServers_Priority",
                table: "McpServers",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_McpUserApiKeys_BackendAuthId_UserId",
                table: "McpUserApiKeys",
                columns: new[] { "McpBackendAuthId", "UserId" },
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_McpUserApiKeys_ExpiresAt",
                table: "McpUserApiKeys",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_McpUserApiKeys_IsDeleted",
                table: "McpUserApiKeys",
                column: "IsDeleted",
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_McpUserApiKeys_IsEnabled",
                table: "McpUserApiKeys",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_McpUserApiKeys_LastUsedAt",
                table: "McpUserApiKeys",
                column: "LastUsedAt");

            migrationBuilder.CreateIndex(
                name: "IX_McpUserApiKeys_UserEmail",
                table: "McpUserApiKeys",
                column: "UserEmail");

            migrationBuilder.CreateIndex(
                name: "IX_McpUserApiKeys_UserId",
                table: "McpUserApiKeys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_McpUserAssignments_ExpiresAt",
                table: "McpUserAssignments",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_McpUserAssignments_IsDeleted",
                table: "McpUserAssignments",
                column: "IsDeleted",
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_McpUserAssignments_IsEnabled",
                table: "McpUserAssignments",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_McpUserAssignments_ServerId_UserId",
                table: "McpUserAssignments",
                columns: new[] { "McpServerId", "UserId" },
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_McpUserAssignments_UserEmail",
                table: "McpUserAssignments",
                column: "UserEmail");

            migrationBuilder.CreateIndex(
                name: "IX_McpUserAssignments_UserId",
                table: "McpUserAssignments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "McpAuditLogs");

            migrationBuilder.DropTable(
                name: "McpRoleAssignments");

            migrationBuilder.DropTable(
                name: "McpUserApiKeys");

            migrationBuilder.DropTable(
                name: "McpUserAssignments");

            migrationBuilder.DropTable(
                name: "RequestMetrics");

            migrationBuilder.DropTable(
                name: "SystemMetrics");

            migrationBuilder.DropTable(
                name: "McpBackendAuths");

            migrationBuilder.DropTable(
                name: "McpServers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Clusters");

            migrationBuilder.DropColumn(
                name: "FilePassword",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Certificates");

            migrationBuilder.AlterColumn<string>(
                name: "SubjectAlternativeNames",
                table: "Certificates",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                oldComment: "Subject Alternative Names for self-signed certificates");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Certificates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "Certificate display name");

            migrationBuilder.AlterColumn<string>(
                name: "KeyVaultUri",
                table: "Certificates",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                oldComment: "Azure Key Vault URI");

            migrationBuilder.AlterColumn<string>(
                name: "KeyVaultName",
                table: "Certificates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "Azure Key Vault name");

            migrationBuilder.AlterColumn<string>(
                name: "KeyVaultCertificatePasswordName",
                table: "Certificates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "Certificate password secret name in Azure Key Vault");

            migrationBuilder.AlterColumn<string>(
                name: "KeyVaultCertificateName",
                table: "Certificates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "Certificate name in Azure Key Vault");

            migrationBuilder.AlterColumn<string>(
                name: "CertificateSource",
                table: "Certificates",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldComment: "Certificate source type: Local, KeyVault, InMemory, SelfSigned");

            migrationBuilder.AddColumn<string>(
                name: "AwsCertificateName",
                table: "Certificates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AwsCertificatePasswordName",
                table: "Certificates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AwsRegion",
                table: "Certificates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}

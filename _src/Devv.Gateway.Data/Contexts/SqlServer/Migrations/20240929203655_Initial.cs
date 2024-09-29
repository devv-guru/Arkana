using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devv.Gateway.Data.Contexts.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CertificateSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KeyVaultName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    KeyVaultUri = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    KeyVaultCertificateName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    KeyVaultCertificatePasswordName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AwsRegion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AwsCertificateName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AwsCertificatePasswordName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebHosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HostName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CertificateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClusterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebHosts_Certificates_CertificateId",
                        column: x => x.CertificateId,
                        principalTable: "Certificates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clusters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoadBalancingPolicy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clusters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clusters_WebHosts_HostId",
                        column: x => x.HostId,
                        principalTable: "WebHosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClusterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: true),
                    MaxRequestBodySize = table.Column<long>(type: "bigint", nullable: true),
                    AuthorizationPolicy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CorsPolicy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Routes_WebHosts_HostId",
                        column: x => x.HostId,
                        principalTable: "WebHosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Destinations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Health = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClusterConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destinations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Destinations_Clusters_ClusterConfigId",
                        column: x => x.ClusterConfigId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthChecks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClusterConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthChecks_Clusters_ClusterConfigId",
                        column: x => x.ClusterConfigId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HttpClientSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SslProtocols = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DangerousAcceptAnyServerCertificate = table.Column<bool>(type: "bit", nullable: false),
                    MaxConnectionsPerServer = table.Column<int>(type: "int", nullable: false),
                    EnableMultipleHttp2Connections = table.Column<bool>(type: "bit", nullable: false),
                    RequestHeaderEncoding = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseHeaderEncoding = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClusterConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HttpClientSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HttpClientSettings_Clusters_ClusterConfigId",
                        column: x => x.ClusterConfigId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HttpRequestSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityTimeout = table.Column<TimeSpan>(type: "time", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionPolicy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AllowResponseBuffering = table.Column<bool>(type: "bit", nullable: false),
                    ClusterConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HttpRequestSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HttpRequestSettings_Clusters_ClusterConfigId",
                        column: x => x.ClusterConfigId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionAffinity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    Policy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FailurePolicy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClusterConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionAffinity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionAffinity_Clusters_ClusterConfigId",
                        column: x => x.ClusterConfigId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Hosts = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Methods = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RouteConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Routes_RouteConfigId",
                        column: x => x.RouteConfigId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Metadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RouteConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClusterConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Metadata_Clusters_ClusterConfigId",
                        column: x => x.ClusterConfigId,
                        principalTable: "Clusters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Metadata_Routes_ClusterConfigId",
                        column: x => x.ClusterConfigId,
                        principalTable: "Routes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transforms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestHeader = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Set = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RouteConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transforms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transforms_Routes_RouteConfigId",
                        column: x => x.RouteConfigId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActiveHealthChecks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    Interval = table.Column<TimeSpan>(type: "time", nullable: false),
                    Timeout = table.Column<TimeSpan>(type: "time", nullable: true),
                    Policy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HealthCheckConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveHealthChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveHealthChecks_HealthChecks_HealthCheckConfigId",
                        column: x => x.HealthCheckConfigId,
                        principalTable: "HealthChecks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PassiveHealthChecks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    Policy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReactivationPeriod = table.Column<TimeSpan>(type: "time", nullable: true),
                    HealthCheckConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PassiveHealthChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PassiveHealthChecks_HealthChecks_HealthCheckConfigId",
                        column: x => x.HealthCheckConfigId,
                        principalTable: "HealthChecks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HeaderMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Values = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCaseSensitive = table.Column<bool>(type: "bit", nullable: false),
                    MatchConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeaderMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeaderMatches_Matches_MatchConfigId",
                        column: x => x.MatchConfigId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QueryParameterMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Values = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCaseSensitive = table.Column<bool>(type: "bit", nullable: false),
                    MatchConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryParameterMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueryParameterMatches_Matches_MatchConfigId",
                        column: x => x.MatchConfigId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveHealthChecks_HealthCheckConfigId",
                table: "ActiveHealthChecks",
                column: "HealthCheckConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_HostId",
                table: "Clusters",
                column: "HostId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Destinations_ClusterConfigId",
                table: "Destinations",
                column: "ClusterConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_HeaderMatches_MatchConfigId",
                table: "HeaderMatches",
                column: "MatchConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthChecks_ClusterConfigId",
                table: "HealthChecks",
                column: "ClusterConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HttpClientSettings_ClusterConfigId",
                table: "HttpClientSettings",
                column: "ClusterConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HttpRequestSettings_ClusterConfigId",
                table: "HttpRequestSettings",
                column: "ClusterConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_RouteConfigId",
                table: "Matches",
                column: "RouteConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Metadata_ClusterConfigId",
                table: "Metadata",
                column: "ClusterConfigId",
                unique: true,
                filter: "[ClusterConfigId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PassiveHealthChecks_HealthCheckConfigId",
                table: "PassiveHealthChecks",
                column: "HealthCheckConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueryParameterMatches_MatchConfigId",
                table: "QueryParameterMatches",
                column: "MatchConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_HostId",
                table: "Routes",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAffinity_ClusterConfigId",
                table: "SessionAffinity",
                column: "ClusterConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transforms_RouteConfigId",
                table: "Transforms",
                column: "RouteConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_WebHosts_CertificateId",
                table: "WebHosts",
                column: "CertificateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveHealthChecks");

            migrationBuilder.DropTable(
                name: "Destinations");

            migrationBuilder.DropTable(
                name: "HeaderMatches");

            migrationBuilder.DropTable(
                name: "HttpClientSettings");

            migrationBuilder.DropTable(
                name: "HttpRequestSettings");

            migrationBuilder.DropTable(
                name: "Metadata");

            migrationBuilder.DropTable(
                name: "PassiveHealthChecks");

            migrationBuilder.DropTable(
                name: "QueryParameterMatches");

            migrationBuilder.DropTable(
                name: "SessionAffinity");

            migrationBuilder.DropTable(
                name: "Transforms");

            migrationBuilder.DropTable(
                name: "HealthChecks");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Clusters");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "WebHosts");

            migrationBuilder.DropTable(
                name: "Certificates");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Contexts.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCertificatesFromWebHost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebHosts_Certificates_CertificateId",
                table: "WebHosts");

            migrationBuilder.DropIndex(
                name: "IX_WebHosts_CertificateId",
                table: "WebHosts");

            migrationBuilder.DropColumn(
                name: "CertificateId",
                table: "WebHosts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CertificateId",
                table: "WebHosts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebHosts_CertificateId",
                table: "WebHosts",
                column: "CertificateId");

            migrationBuilder.AddForeignKey(
                name: "FK_WebHosts_Certificates_CertificateId",
                table: "WebHosts",
                column: "CertificateId",
                principalTable: "Certificates",
                principalColumn: "Id");
        }
    }
}

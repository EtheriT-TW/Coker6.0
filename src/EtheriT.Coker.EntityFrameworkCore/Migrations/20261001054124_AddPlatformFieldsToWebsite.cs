using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformFieldsToWebsite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FK_PlatformDomainId",
                table: "Websites",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HostLocation",
                table: "Websites",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDomainPending",
                table: "Websites",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PlatformRemark",
                table: "Websites",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceStatus",
                table: "Websites",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "TerminatedDate",
                table: "Websites",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Websites",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "FK_PlatformDomainId", "HostLocation", "IsDomainPending", "PlatformRemark", "ServiceStatus", "TerminatedDate" },
                values: new object[] { null, null, false, null, 0, null });

            migrationBuilder.UpdateData(
                table: "Websites",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "FK_PlatformDomainId", "HostLocation", "IsDomainPending", "PlatformRemark", "ServiceStatus", "TerminatedDate" },
                values: new object[] { null, null, false, null, 0, null });

            migrationBuilder.CreateIndex(
                name: "IX_Websites_EndDate",
                table: "Websites",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Websites_FK_PlatformDomainId",
                table: "Websites",
                column: "FK_PlatformDomainId");

            migrationBuilder.CreateIndex(
                name: "IX_Websites_ServiceStatus",
                table: "Websites",
                column: "ServiceStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_Websites_PlatformDomains_FK_PlatformDomainId",
                table: "Websites",
                column: "FK_PlatformDomainId",
                principalTable: "PlatformDomains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Websites_PlatformDomains_FK_PlatformDomainId",
                table: "Websites");

            migrationBuilder.DropIndex(
                name: "IX_Websites_EndDate",
                table: "Websites");

            migrationBuilder.DropIndex(
                name: "IX_Websites_FK_PlatformDomainId",
                table: "Websites");

            migrationBuilder.DropIndex(
                name: "IX_Websites_ServiceStatus",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "FK_PlatformDomainId",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "HostLocation",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "IsDomainPending",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "PlatformRemark",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "ServiceStatus",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "TerminatedDate",
                table: "Websites");
        }
    }
}

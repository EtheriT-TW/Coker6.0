using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformDomains : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DomainEndDate",
                table: "PlatformWebsites");

            migrationBuilder.DropColumn(
                name: "DomainName",
                table: "PlatformWebsites");

            migrationBuilder.DropColumn(
                name: "DomainPasswordCipher",
                table: "PlatformWebsites");

            migrationBuilder.DropColumn(
                name: "DomainRegistrar",
                table: "PlatformWebsites");

            migrationBuilder.DropColumn(
                name: "DomainStartDate",
                table: "PlatformWebsites");

            migrationBuilder.AddColumn<long>(
                name: "FK_PlatformDomainId",
                table: "PlatformWebsites",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "PlatformWebsites",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlatformDomains",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DomainName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Registrar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordCipher = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformDomains", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWebsites_FK_PlatformDomainId",
                table: "PlatformWebsites",
                column: "FK_PlatformDomainId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformDomains_DomainName",
                table: "PlatformDomains",
                column: "DomainName",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformDomains_EndDate",
                table: "PlatformDomains",
                column: "EndDate");

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformWebsites_PlatformDomains_FK_PlatformDomainId",
                table: "PlatformWebsites",
                column: "FK_PlatformDomainId",
                principalTable: "PlatformDomains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlatformWebsites_PlatformDomains_FK_PlatformDomainId",
                table: "PlatformWebsites");

            migrationBuilder.DropTable(
                name: "PlatformDomains");

            migrationBuilder.DropIndex(
                name: "IX_PlatformWebsites_FK_PlatformDomainId",
                table: "PlatformWebsites");

            migrationBuilder.DropColumn(
                name: "FK_PlatformDomainId",
                table: "PlatformWebsites");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "PlatformWebsites");

            migrationBuilder.AddColumn<DateTime>(
                name: "DomainEndDate",
                table: "PlatformWebsites",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DomainName",
                table: "PlatformWebsites",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DomainPasswordCipher",
                table: "PlatformWebsites",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DomainRegistrar",
                table: "PlatformWebsites",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DomainStartDate",
                table: "PlatformWebsites",
                type: "datetime2",
                nullable: true);
        }
    }
}

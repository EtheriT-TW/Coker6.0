using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class PlatformCustomersToCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM PlatformWebsites;");

            migrationBuilder.DropForeignKey(
                name: "FK_PlatformWebsites_PlatformCustomers_FK_PlatformCustomerId",
                table: "PlatformWebsites");

            migrationBuilder.DropTable(
                name: "PlatformCustomerContacts");

            migrationBuilder.DropTable(
                name: "PlatformCustomers");

            migrationBuilder.RenameColumn(
                name: "FK_PlatformCustomerId",
                table: "PlatformWebsites",
                newName: "FK_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_PlatformWebsites_FK_PlatformCustomerId",
                table: "PlatformWebsites",
                newName: "IX_PlatformWebsites_FK_CompanyId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Companies",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Companies",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactJobTitle",
                table: "Companies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Companies",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerType",
                table: "Companies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CustomerTypeOther",
                table: "Companies",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceInfo",
                table: "Companies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Companies",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesOwner",
                table: "Companies",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SecondaryContacts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Sort = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_SecondaryContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecondaryContacts_Companies_FK_CompanyId",
                        column: x => x.FK_CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Name",
                table: "Companies",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_TaxID",
                table: "Companies",
                column: "TaxID",
                unique: true,
                filter: "[IsDeleted] = 0 AND [TaxID] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_SecondaryContacts_FK_CompanyId_Sort",
                table: "SecondaryContacts",
                columns: new[] { "FK_CompanyId", "Sort" });

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformWebsites_Companies_FK_CompanyId",
                table: "PlatformWebsites",
                column: "FK_CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlatformWebsites_Companies_FK_CompanyId",
                table: "PlatformWebsites");

            migrationBuilder.DropTable(
                name: "SecondaryContacts");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Name",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_TaxID",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ContactJobTitle",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CustomerType",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CustomerTypeOther",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "InvoiceInfo",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SalesOwner",
                table: "Companies");

            migrationBuilder.RenameColumn(
                name: "FK_CompanyId",
                table: "PlatformWebsites",
                newName: "FK_PlatformCustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_PlatformWebsites_FK_CompanyId",
                table: "PlatformWebsites",
                newName: "IX_PlatformWebsites_FK_PlatformCustomerId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateTable(
                name: "PlatformCustomers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerType = table.Column<int>(type: "int", nullable: false),
                    CustomerTypeOther = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    InvoiceInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrimaryContactEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PrimaryContactJobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PrimaryContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PrimaryContactPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SalesOwner = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TaxId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformCustomers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformCustomerContacts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_PlatformCustomerId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    JobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Sort = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformCustomerContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformCustomerContacts_PlatformCustomers_FK_PlatformCustomerId",
                        column: x => x.FK_PlatformCustomerId,
                        principalTable: "PlatformCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformCustomerContacts_FK_PlatformCustomerId_Sort",
                table: "PlatformCustomerContacts",
                columns: new[] { "FK_PlatformCustomerId", "Sort" });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformCustomers_Name",
                table: "PlatformCustomers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformCustomers_TaxId_IsDeleted",
                table: "PlatformCustomers",
                columns: new[] { "TaxId", "IsDeleted" });

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformWebsites_PlatformCustomers_FK_PlatformCustomerId",
                table: "PlatformWebsites",
                column: "FK_PlatformCustomerId",
                principalTable: "PlatformCustomers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

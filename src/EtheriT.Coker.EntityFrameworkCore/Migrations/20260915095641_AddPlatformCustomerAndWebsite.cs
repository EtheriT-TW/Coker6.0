using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformCustomerAndWebsite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlatformCustomers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaxId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    InvoiceInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SalesOwner = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CustomerType = table.Column<int>(type: "int", nullable: false),
                    CustomerTypeOther = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrimaryContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PrimaryContactJobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PrimaryContactPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrimaryContactEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
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
                    table.PrimaryKey("PK_PlatformCustomers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformCustomerContacts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_PlatformCustomerId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_PlatformCustomerContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformCustomerContacts_PlatformCustomers_FK_PlatformCustomerId",
                        column: x => x.FK_PlatformCustomerId,
                        principalTable: "PlatformCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlatformWebsites",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_PlatformCustomerId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Level = table.Column<int>(type: "int", nullable: true),
                    HostLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ServiceStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ServiceEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TerminatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDomainPending = table.Column<bool>(type: "bit", nullable: false),
                    DomainName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DomainRegistrar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DomainStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DomainEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DomainPasswordCipher = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_PlatformWebsites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformWebsites_PlatformCustomers_FK_PlatformCustomerId",
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

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWebsites_FK_PlatformCustomerId",
                table: "PlatformWebsites",
                column: "FK_PlatformCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWebsites_ServiceEndDate",
                table: "PlatformWebsites",
                column: "ServiceEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWebsites_Status",
                table: "PlatformWebsites",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformCustomerContacts");

            migrationBuilder.DropTable(
                name: "PlatformWebsites");

            migrationBuilder.DropTable(
                name: "PlatformCustomers");
        }
    }
}

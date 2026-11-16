using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class CompleteCustSearchSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustSearch_FK_WebsiteId",
                table: "CustSearch");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CustSearch",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "CustSearch",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ScopeJson",
                table: "CustSearch",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SystemTargetId",
                table: "CustSearch",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustSearch_FK_WebsiteId_IsDefault",
                table: "CustSearch",
                columns: new[] { "FK_WebsiteId", "IsDefault" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [IsDefault] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CustSearch_FK_WebsiteId_SystemTargetId",
                table: "CustSearch",
                columns: new[] { "FK_WebsiteId", "SystemTargetId" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [SystemTargetId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustSearch_FK_WebsiteId_IsDefault",
                table: "CustSearch");

            migrationBuilder.DropIndex(
                name: "IX_CustSearch_FK_WebsiteId_SystemTargetId",
                table: "CustSearch");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CustSearch");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "CustSearch");

            migrationBuilder.DropColumn(
                name: "ScopeJson",
                table: "CustSearch");

            migrationBuilder.DropColumn(
                name: "SystemTargetId",
                table: "CustSearch");

            migrationBuilder.CreateIndex(
                name: "IX_CustSearch_FK_WebsiteId",
                table: "CustSearch",
                column: "FK_WebsiteId");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class AlterTable_AuditLogs_WebId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WebId",
                table: "AuditLogs");

            migrationBuilder.AddColumn<long>(
                name: "FK_WebsiteId",
                table: "AuditLogs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_FK_WebsiteId",
                table: "AuditLogs",
                column: "FK_WebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Websites_FK_WebsiteId",
                table: "AuditLogs",
                column: "FK_WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Websites_FK_WebsiteId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_FK_WebsiteId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "FK_WebsiteId",
                table: "AuditLogs");

            migrationBuilder.AddColumn<int>(
                name: "WebId",
                table: "AuditLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

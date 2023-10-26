using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alter_Table_AuditLog_UserId_FK_WebsiteId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Websites_FK_WebsiteId",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "AuditLogs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "FK_WebsiteId",
                table: "AuditLogs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Websites_FK_WebsiteId",
                table: "AuditLogs",
                column: "FK_WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Websites_FK_WebsiteId",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "AuditLogs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "FK_WebsiteId",
                table: "AuditLogs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Websites_FK_WebsiteId",
                table: "AuditLogs",
                column: "FK_WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

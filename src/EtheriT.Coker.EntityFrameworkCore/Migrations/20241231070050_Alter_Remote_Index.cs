using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alter_Remote_Index : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Remotes_ExecutionTime",
                table: "Remotes",
                column: "ExecutionTime");

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_FK_WebsiteId",
                table: "Remotes",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_UUID",
                table: "Remotes",
                column: "UUID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Remotes_ExecutionTime",
                table: "Remotes");

            migrationBuilder.DropIndex(
                name: "IX_Remotes_FK_WebsiteId",
                table: "Remotes");

            migrationBuilder.DropIndex(
                name: "IX_Remotes_UUID",
                table: "Remotes");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_Marketing_Add_FKWebsite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FK_WebsiteId",
                table: "Marketing",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Marketing_FK_WebsiteId",
                table: "Marketing",
                column: "FK_WebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Marketing_Websites_FK_WebsiteId",
                table: "Marketing",
                column: "FK_WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Marketing_Websites_FK_WebsiteId",
                table: "Marketing");

            migrationBuilder.DropIndex(
                name: "IX_Marketing_FK_WebsiteId",
                table: "Marketing");

            migrationBuilder.DropColumn(
                name: "FK_WebsiteId",
                table: "Marketing");
        }
    }
}

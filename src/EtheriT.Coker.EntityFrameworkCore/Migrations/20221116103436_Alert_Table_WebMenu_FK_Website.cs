using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alert_Table_WebMenu_FK_Website : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WebMenus_FK_WebsiteId",
                table: "WebMenus",
                column: "FK_WebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_WebMenus_Websites_FK_WebsiteId",
                table: "WebMenus",
                column: "FK_WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebMenus_Websites_FK_WebsiteId",
                table: "WebMenus");

            migrationBuilder.DropIndex(
                name: "IX_WebMenus_FK_WebsiteId",
                table: "WebMenus");
        }
    }
}

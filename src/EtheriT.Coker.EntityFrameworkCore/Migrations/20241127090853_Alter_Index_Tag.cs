using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alter_Index_Tag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WebMenus_Title",
                table: "WebMenus",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Title_FK_WebsiteId",
                table: "Tags",
                columns: new[] { "Title", "FK_WebsiteId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Prods_Title",
                table: "Prods",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Article_Title",
                table: "Article",
                column: "Title");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WebMenus_Title",
                table: "WebMenus");

            migrationBuilder.DropIndex(
                name: "IX_Tags_Title_FK_WebsiteId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Prods_Title",
                table: "Prods");

            migrationBuilder.DropIndex(
                name: "IX_Article_Title",
                table: "Article");
        }
    }
}

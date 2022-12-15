using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Menus_RootNodeId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "WebMenus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FK_RootNodeId",
                table: "WebMenus",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebMenus_FK_RootNodeId",
                table: "WebMenus",
                column: "FK_RootNodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_WebMenus_WebMenus_FK_RootNodeId",
                table: "WebMenus",
                column: "FK_RootNodeId",
                principalTable: "WebMenus",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebMenus_WebMenus_FK_RootNodeId",
                table: "WebMenus");

            migrationBuilder.DropIndex(
                name: "IX_WebMenus_FK_RootNodeId",
                table: "WebMenus");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "WebMenus");

            migrationBuilder.DropColumn(
                name: "FK_RootNodeId",
                table: "WebMenus");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class AlterTable_WebMenu_VisibleTitle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "VisibleTitle",
                table: "WebMenus",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VisibleTitle",
                table: "WebMenus");
        }
    }
}

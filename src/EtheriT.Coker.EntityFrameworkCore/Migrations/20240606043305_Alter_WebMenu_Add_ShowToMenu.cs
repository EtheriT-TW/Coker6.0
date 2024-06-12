using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alter_WebMenu_Add_ShowToMenu : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Disp_Opt",
                table: "Prods");

            migrationBuilder.AddColumn<bool>(
                name: "ShowToMenu",
                table: "WebMenus",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowToMenu",
                table: "WebMenus");

            migrationBuilder.AddColumn<bool>(
                name: "Disp_Opt",
                table: "Prods",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}

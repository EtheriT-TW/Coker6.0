using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Menus_Contents_css_html_column : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Target",
                table: "WebMenus",
                type: "bit",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Css",
                table: "WebMenus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Html",
                table: "WebMenus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaveCss",
                table: "WebMenus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaveHtml",
                table: "WebMenus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "icone",
                table: "WebMenus",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Css",
                table: "WebMenus");

            migrationBuilder.DropColumn(
                name: "Html",
                table: "WebMenus");

            migrationBuilder.DropColumn(
                name: "SaveCss",
                table: "WebMenus");

            migrationBuilder.DropColumn(
                name: "SaveHtml",
                table: "WebMenus");

            migrationBuilder.DropColumn(
                name: "icone",
                table: "WebMenus");

            migrationBuilder.AlterColumn<string>(
                name: "Target",
                table: "WebMenus",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);
        }
    }
}

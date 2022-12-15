using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Contents_css_html_column : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Html_Contents",
                newName: "Html");

            migrationBuilder.AddColumn<string>(
                name: "Css",
                table: "Html_Contents",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Css",
                table: "Html_Contents");

            migrationBuilder.RenameColumn(
                name: "Html",
                table: "Html_Contents",
                newName: "Content");
        }
    }
}

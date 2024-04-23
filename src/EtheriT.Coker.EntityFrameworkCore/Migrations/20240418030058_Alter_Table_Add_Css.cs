using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alter_Table_Add_Css : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "html",
                table: "TechnicalCertificates",
                newName: "Html");

            migrationBuilder.AddColumn<string>(
                name: "Css",
                table: "TechnicalCertificates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Css",
                table: "TechnicalCertificates");

            migrationBuilder.RenameColumn(
                name: "Html",
                table: "TechnicalCertificates",
                newName: "html");
        }
    }
}

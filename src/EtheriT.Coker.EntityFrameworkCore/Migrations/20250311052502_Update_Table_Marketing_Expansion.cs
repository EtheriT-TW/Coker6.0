using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_Marketing_Expansion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Enable",
                table: "Marketing",
                newName: "IsReusable");

            migrationBuilder.AddColumn<int>(
                name: "ActivityType",
                table: "Marketing",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Marketing",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Marketing",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Marketing",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivityType",
                table: "Marketing");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Marketing");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Marketing");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Marketing");

            migrationBuilder.RenameColumn(
                name: "IsReusable",
                table: "Marketing",
                newName: "Enable");
        }
    }
}

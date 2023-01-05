using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alter_Website_OrgName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrgName",
                table: "Websites",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "Size",
                table: "FileUploads",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Websites",
                keyColumn: "Id",
                keyValue: 1L,
                column: "OrgName",
                value: "coker6");

            migrationBuilder.UpdateData(
                table: "Websites",
                keyColumn: "Id",
                keyValue: 2L,
                column: "OrgName",
                value: "lcb");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrgName",
                table: "Websites");

            migrationBuilder.AlterColumn<int>(
                name: "Size",
                table: "FileUploads",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}

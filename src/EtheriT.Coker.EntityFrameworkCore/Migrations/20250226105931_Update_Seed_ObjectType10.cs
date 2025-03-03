using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Seed_ObjectType10 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 10L,
                column: "Title",
                value: "多欄位編排");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 10L,
                column: "Title",
                value: "進階(程式)");
        }
    }
}

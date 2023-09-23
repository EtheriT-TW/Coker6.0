using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alter_Table_StoreSet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "job_id",
                table: "StoreSetDetail");

            migrationBuilder.AddColumn<string>(
                name: "jobID",
                table: "StoreSet",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "jobID", "type" },
                values: new object[] { "S001", 7 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "jobID",
                table: "StoreSet");

            migrationBuilder.AddColumn<string>(
                name: "job_id",
                table: "StoreSetDetail",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 1L,
                column: "type",
                value: 1);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Table_CustSearch_FK_WebsiteID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SerNo",
                table: "CustSearch",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FK_WebsiteId",
                table: "CustSearch",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "Visible",
                table: "CustSearch",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_CustSearch_FK_WebsiteId",
                table: "CustSearch",
                column: "FK_WebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustSearch_Websites_FK_WebsiteId",
                table: "CustSearch",
                column: "FK_WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustSearch_Websites_FK_WebsiteId",
                table: "CustSearch");

            migrationBuilder.DropIndex(
                name: "IX_CustSearch_FK_WebsiteId",
                table: "CustSearch");

            migrationBuilder.DropColumn(
                name: "FK_WebsiteId",
                table: "CustSearch");

            migrationBuilder.DropColumn(
                name: "Visible",
                table: "CustSearch");

            migrationBuilder.AlterColumn<int>(
                name: "SerNo",
                table: "CustSearch",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}

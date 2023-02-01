using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alter_ObjectTypes_FK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Html_Contents_Type",
                table: "Html_Contents",
                column: "Type");

            migrationBuilder.AddForeignKey(
                name: "FK_Html_Contents_ObjectTypes_Type",
                table: "Html_Contents",
                column: "Type",
                principalTable: "ObjectTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Html_Contents_ObjectTypes_Type",
                table: "Html_Contents");

            migrationBuilder.DropIndex(
                name: "IX_Html_Contents_Type",
                table: "Html_Contents");
        }
    }
}

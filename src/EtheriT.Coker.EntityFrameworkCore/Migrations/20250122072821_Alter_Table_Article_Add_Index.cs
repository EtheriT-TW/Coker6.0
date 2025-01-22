using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alter_Table_Article_Add_Index : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Article_EndTime",
                table: "Article",
                column: "EndTime");

            migrationBuilder.CreateIndex(
                name: "IX_Article_NodeDate",
                table: "Article",
                column: "NodeDate");

            migrationBuilder.CreateIndex(
                name: "IX_Article_permanent",
                table: "Article",
                column: "permanent");

            migrationBuilder.CreateIndex(
                name: "IX_Article_RemovedFromShelves",
                table: "Article",
                column: "RemovedFromShelves");

            migrationBuilder.CreateIndex(
                name: "IX_Article_SerNO",
                table: "Article",
                column: "SerNO");

            migrationBuilder.CreateIndex(
                name: "IX_Article_StartTime",
                table: "Article",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Article_Visible",
                table: "Article",
                column: "Visible");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Article_EndTime",
                table: "Article");

            migrationBuilder.DropIndex(
                name: "IX_Article_NodeDate",
                table: "Article");

            migrationBuilder.DropIndex(
                name: "IX_Article_permanent",
                table: "Article");

            migrationBuilder.DropIndex(
                name: "IX_Article_RemovedFromShelves",
                table: "Article");

            migrationBuilder.DropIndex(
                name: "IX_Article_SerNO",
                table: "Article");

            migrationBuilder.DropIndex(
                name: "IX_Article_StartTime",
                table: "Article");

            migrationBuilder.DropIndex(
                name: "IX_Article_Visible",
                table: "Article");
        }
    }
}

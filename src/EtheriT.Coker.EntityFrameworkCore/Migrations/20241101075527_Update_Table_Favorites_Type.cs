using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_Favorites_Type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Prods_FK_PId",
                table: "Favorites");

            migrationBuilder.DropIndex(
                name: "IX_Favorites_FK_PId",
                table: "Favorites");

            migrationBuilder.RenameColumn(
                name: "FK_PId",
                table: "Favorites",
                newName: "FK_AssocId");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Favorites",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Favorites");

            migrationBuilder.RenameColumn(
                name: "FK_AssocId",
                table: "Favorites",
                newName: "FK_PId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_FK_PId",
                table: "Favorites",
                column: "FK_PId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Prods_FK_PId",
                table: "Favorites",
                column: "FK_PId",
                principalTable: "Prods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

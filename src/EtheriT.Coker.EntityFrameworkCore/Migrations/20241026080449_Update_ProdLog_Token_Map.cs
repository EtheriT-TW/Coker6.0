using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_ProdLog_Token_Map : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TokenMapProd_Log_Tokens_id",
                table: "TokenMapProd_Log");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "TokenMapProd_Log",
                newName: "UUID");

            migrationBuilder.AddForeignKey(
                name: "FK_TokenMapProd_Log_Tokens_UUID",
                table: "TokenMapProd_Log",
                column: "UUID",
                principalTable: "Tokens",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TokenMapProd_Log_Tokens_UUID",
                table: "TokenMapProd_Log");

            migrationBuilder.RenameColumn(
                name: "UUID",
                table: "TokenMapProd_Log",
                newName: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_TokenMapProd_Log_Tokens_id",
                table: "TokenMapProd_Log",
                column: "id",
                principalTable: "Tokens",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

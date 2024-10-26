using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_ProdLog_Remove_Token : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokenMapProd_Log");

            migrationBuilder.DropColumn(
                name: "FK_TokenId",
                table: "Prod_Logs");

            migrationBuilder.AddColumn<Guid>(
                name: "Tokenid",
                table: "Prod_Logs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Logs_Tokenid",
                table: "Prod_Logs",
                column: "Tokenid");

            migrationBuilder.AddForeignKey(
                name: "FK_Prod_Logs_Tokens_Tokenid",
                table: "Prod_Logs",
                column: "Tokenid",
                principalTable: "Tokens",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prod_Logs_Tokens_Tokenid",
                table: "Prod_Logs");

            migrationBuilder.DropIndex(
                name: "IX_Prod_Logs_Tokenid",
                table: "Prod_Logs");

            migrationBuilder.DropColumn(
                name: "Tokenid",
                table: "Prod_Logs");

            migrationBuilder.AddColumn<Guid>(
                name: "FK_TokenId",
                table: "Prod_Logs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "TokenMapProd_Log",
                columns: table => new
                {
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_TokenId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenMapProd_Log", x => new { x.UUID, x.FK_TokenId });
                    table.ForeignKey(
                        name: "FK_TokenMapProd_Log_Prod_Logs_FK_TokenId",
                        column: x => x.FK_TokenId,
                        principalTable: "Prod_Logs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TokenMapProd_Log_Tokens_UUID",
                        column: x => x.UUID,
                        principalTable: "Tokens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TokenMapProd_Log_FK_TokenId",
                table: "TokenMapProd_Log",
                column: "FK_TokenId");
        }
    }
}

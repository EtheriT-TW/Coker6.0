using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_ProdLog_UUID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prod_Logs_Users_FK_Uid",
                table: "Prod_Logs");

            migrationBuilder.DropForeignKey(
                name: "FK_TokenMapProd_Log_Prod_Logs_FK_Tid",
                table: "TokenMapProd_Log");

            migrationBuilder.DropForeignKey(
                name: "FK_TokenMapProd_Log_Tokens_UUID",
                table: "TokenMapProd_Log");

            migrationBuilder.RenameColumn(
                name: "FK_Tid",
                table: "TokenMapProd_Log",
                newName: "FK_TokenId");

            migrationBuilder.RenameColumn(
                name: "UUID",
                table: "TokenMapProd_Log",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_TokenMapProd_Log_FK_Tid",
                table: "TokenMapProd_Log",
                newName: "IX_TokenMapProd_Log_FK_TokenId");

            migrationBuilder.RenameColumn(
                name: "FK_Uid",
                table: "Prod_Logs",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "FK_Tid",
                table: "Prod_Logs",
                newName: "UUID");

            migrationBuilder.RenameIndex(
                name: "IX_Prod_Logs_FK_Uid",
                table: "Prod_Logs",
                newName: "IX_Prod_Logs_UserId");

            migrationBuilder.AddColumn<int>(
                name: "Clicks",
                table: "Prods",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FK_TokenId",
                table: "Prod_Logs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "FK_UserId",
                table: "Prod_Logs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Prod_Logs_Users_UserId",
                table: "Prod_Logs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TokenMapProd_Log_Prod_Logs_FK_TokenId",
                table: "TokenMapProd_Log",
                column: "FK_TokenId",
                principalTable: "Prod_Logs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TokenMapProd_Log_Tokens_id",
                table: "TokenMapProd_Log",
                column: "id",
                principalTable: "Tokens",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prod_Logs_Users_UserId",
                table: "Prod_Logs");

            migrationBuilder.DropForeignKey(
                name: "FK_TokenMapProd_Log_Prod_Logs_FK_TokenId",
                table: "TokenMapProd_Log");

            migrationBuilder.DropForeignKey(
                name: "FK_TokenMapProd_Log_Tokens_id",
                table: "TokenMapProd_Log");

            migrationBuilder.DropColumn(
                name: "Clicks",
                table: "Prods");

            migrationBuilder.DropColumn(
                name: "FK_TokenId",
                table: "Prod_Logs");

            migrationBuilder.DropColumn(
                name: "FK_UserId",
                table: "Prod_Logs");

            migrationBuilder.RenameColumn(
                name: "FK_TokenId",
                table: "TokenMapProd_Log",
                newName: "FK_Tid");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "TokenMapProd_Log",
                newName: "UUID");

            migrationBuilder.RenameIndex(
                name: "IX_TokenMapProd_Log_FK_TokenId",
                table: "TokenMapProd_Log",
                newName: "IX_TokenMapProd_Log_FK_Tid");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Prod_Logs",
                newName: "FK_Uid");

            migrationBuilder.RenameColumn(
                name: "UUID",
                table: "Prod_Logs",
                newName: "FK_Tid");

            migrationBuilder.RenameIndex(
                name: "IX_Prod_Logs_UserId",
                table: "Prod_Logs",
                newName: "IX_Prod_Logs_FK_Uid");

            migrationBuilder.AddForeignKey(
                name: "FK_Prod_Logs_Users_FK_Uid",
                table: "Prod_Logs",
                column: "FK_Uid",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TokenMapProd_Log_Prod_Logs_FK_Tid",
                table: "TokenMapProd_Log",
                column: "FK_Tid",
                principalTable: "Prod_Logs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TokenMapProd_Log_Tokens_UUID",
                table: "TokenMapProd_Log",
                column: "UUID",
                principalTable: "Tokens",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

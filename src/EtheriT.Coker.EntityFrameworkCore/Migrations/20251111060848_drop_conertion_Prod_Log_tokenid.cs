using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class drop_conertion_Prod_Log_tokenid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prod_Logs_Tokens_Tokenid",
                table: "Prod_Logs");

            migrationBuilder.DropForeignKey(
                name: "FK_Prod_Logs_Users_UserId",
                table: "Prod_Logs");

            migrationBuilder.DropIndex(
                name: "IX_Prod_Logs_Tokenid",
                table: "Prod_Logs");

            migrationBuilder.DropIndex(
                name: "IX_Prod_Logs_UserId",
                table: "Prod_Logs");

            migrationBuilder.DropColumn(
                name: "Tokenid",
                table: "Prod_Logs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Prod_Logs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Tokenid",
                table: "Prod_Logs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Prod_Logs",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Logs_Tokenid",
                table: "Prod_Logs",
                column: "Tokenid");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Logs_UserId",
                table: "Prod_Logs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prod_Logs_Tokens_Tokenid",
                table: "Prod_Logs",
                column: "Tokenid",
                principalTable: "Tokens",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Prod_Logs_Users_UserId",
                table: "Prod_Logs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}

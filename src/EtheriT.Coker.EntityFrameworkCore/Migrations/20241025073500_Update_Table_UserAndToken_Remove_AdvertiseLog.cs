using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_UserAndToken_Remove_AdvertiseLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Advertise_Logs_Tokens_Tokenid",
                table: "Advertise_Logs");

            migrationBuilder.DropForeignKey(
                name: "FK_Advertise_Logs_Users_UserId",
                table: "Advertise_Logs");

            migrationBuilder.DropIndex(
                name: "IX_Advertise_Logs_Tokenid",
                table: "Advertise_Logs");

            migrationBuilder.DropIndex(
                name: "IX_Advertise_Logs_UserId",
                table: "Advertise_Logs");

            migrationBuilder.DropColumn(
                name: "Tokenid",
                table: "Advertise_Logs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Advertise_Logs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Tokenid",
                table: "Advertise_Logs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Advertise_Logs",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Advertise_Logs_Tokenid",
                table: "Advertise_Logs",
                column: "Tokenid");

            migrationBuilder.CreateIndex(
                name: "IX_Advertise_Logs_UserId",
                table: "Advertise_Logs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertise_Logs_Tokens_Tokenid",
                table: "Advertise_Logs",
                column: "Tokenid",
                principalTable: "Tokens",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertise_Logs_Users_UserId",
                table: "Advertise_Logs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}

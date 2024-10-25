using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_AdvertiseLog_UUID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Advertise_Logs_Users_FK_Uid",
                table: "Advertise_Logs");

            migrationBuilder.DropTable(
                name: "TokenMapAdvertise_Log");

            migrationBuilder.RenameColumn(
                name: "FK_Uid",
                table: "Advertise_Logs",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "FK_Tid",
                table: "Advertise_Logs",
                newName: "Tokenid");

            migrationBuilder.RenameIndex(
                name: "IX_Advertise_Logs_FK_Uid",
                table: "Advertise_Logs",
                newName: "IX_Advertise_Logs_UserId");

            migrationBuilder.AddColumn<long>(
                name: "FK_UserId",
                table: "Advertise_Logs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UUID",
                table: "Advertise_Logs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Advertise_Logs_Tokenid",
                table: "Advertise_Logs",
                column: "Tokenid");

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

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "FK_UserId",
                table: "Advertise_Logs");

            migrationBuilder.DropColumn(
                name: "UUID",
                table: "Advertise_Logs");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Advertise_Logs",
                newName: "FK_Uid");

            migrationBuilder.RenameColumn(
                name: "Tokenid",
                table: "Advertise_Logs",
                newName: "FK_Tid");

            migrationBuilder.RenameIndex(
                name: "IX_Advertise_Logs_UserId",
                table: "Advertise_Logs",
                newName: "IX_Advertise_Logs_FK_Uid");

            migrationBuilder.CreateTable(
                name: "TokenMapAdvertise_Log",
                columns: table => new
                {
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_Tid = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenMapAdvertise_Log", x => new { x.UUID, x.FK_Tid });
                    table.ForeignKey(
                        name: "FK_TokenMapAdvertise_Log_Advertise_Logs_FK_Tid",
                        column: x => x.FK_Tid,
                        principalTable: "Advertise_Logs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TokenMapAdvertise_Log_Tokens_UUID",
                        column: x => x.UUID,
                        principalTable: "Tokens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TokenMapAdvertise_Log_FK_Tid",
                table: "TokenMapAdvertise_Log",
                column: "FK_Tid");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertise_Logs_Users_FK_Uid",
                table: "Advertise_Logs",
                column: "FK_Uid",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}

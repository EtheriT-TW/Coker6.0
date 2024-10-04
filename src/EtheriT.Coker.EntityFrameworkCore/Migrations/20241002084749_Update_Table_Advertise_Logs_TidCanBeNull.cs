using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_Advertise_Logs_TidCanBeNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Advertise_Logs_Tokens_FK_Tid",
                table: "Advertise_Logs");

            migrationBuilder.AlterColumn<Guid>(
                name: "FK_Tid",
                table: "Advertise_Logs",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertise_Logs_Tokens_FK_Tid",
                table: "Advertise_Logs",
                column: "FK_Tid",
                principalTable: "Tokens",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Advertise_Logs_Tokens_FK_Tid",
                table: "Advertise_Logs");

            migrationBuilder.AlterColumn<Guid>(
                name: "FK_Tid",
                table: "Advertise_Logs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Advertise_Logs_Tokens_FK_Tid",
                table: "Advertise_Logs",
                column: "FK_Tid",
                principalTable: "Tokens",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

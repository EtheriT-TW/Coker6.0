using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_FrontUser_Token_Add_PrivacyAgreeTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PrivacyAgreeTime",
                table: "Tokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrivacyAgreeTime",
                table: "FrontUsers",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivacyAgreeTime",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "PrivacyAgreeTime",
                table: "FrontUsers");
        }
    }
}

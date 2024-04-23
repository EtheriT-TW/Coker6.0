using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alert_Table_Contacts_And_TechnicalCertificates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Contacts",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "AskJson",
                table: "Contacts",
                newName: "Html");

            migrationBuilder.AddColumn<string>(
                name: "html",
                table: "TechnicalCertificates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReplyTime",
                table: "Contacts",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "TargetEmail",
                table: "Contacts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "html",
                table: "TechnicalCertificates");

            migrationBuilder.DropColumn(
                name: "TargetEmail",
                table: "Contacts");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Contacts",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Html",
                table: "Contacts",
                newName: "AskJson");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReplyTime",
                table: "Contacts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}

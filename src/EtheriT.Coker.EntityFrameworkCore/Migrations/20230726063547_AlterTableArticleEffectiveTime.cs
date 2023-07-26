using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class AlterTableArticleEffectiveTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "Article",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "Article",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "permanent",
                table: "Article",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "permanent",
                table: "Article");
        }
    }
}

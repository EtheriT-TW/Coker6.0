using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_WebsiteUserOrder_Header : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "Websites",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contract",
                table: "Websites",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Websites",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "Websites",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Websites",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Statement",
                table: "Websites",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UniformId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrdererSex",
                table: "Order_Headers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contact",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "Contract",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "Logo",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "Statement",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "UniformId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrdererSex",
                table: "Order_Headers");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_OrderHeader_Add_UUIDAndToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOrder",
                table: "ShoppingCarts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "FK_UUID",
                table: "Order_Headers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Fk_Tid",
                table: "Order_Headers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "Fk_UserId",
                table: "Order_Headers",
                type: "bigint",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOrder",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "FK_UUID",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "Fk_Tid",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "Fk_UserId",
                table: "Order_Headers");
        }
    }
}

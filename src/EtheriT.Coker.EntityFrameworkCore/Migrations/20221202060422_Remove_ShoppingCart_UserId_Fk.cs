using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Remove_ShoppingCart_UserId_Fk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_Users_FK_Uid",
                table: "ShoppingCarts");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCarts_FK_Uid",
                table: "ShoppingCarts");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_FK_Uid",
                table: "ShoppingCarts",
                column: "FK_Uid");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_Users_FK_Uid",
                table: "ShoppingCarts",
                column: "FK_Uid",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_ShoppingCart_Add_Fk_PriceId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FK_PriceId",
                table: "ShoppingCarts",
                type: "bigint",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 10L,
                column: "Title",
                value: "進階(程式)");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_FK_PriceId",
                table: "ShoppingCarts",
                column: "FK_PriceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_Prod_Prices_FK_PriceId",
                table: "ShoppingCarts",
                column: "FK_PriceId",
                principalTable: "Prod_Prices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_Prod_Prices_FK_PriceId",
                table: "ShoppingCarts");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCarts_FK_PriceId",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "FK_PriceId",
                table: "ShoppingCarts");

            migrationBuilder.UpdateData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 10L,
                column: "Title",
                value: "目錄");
        }
    }
}

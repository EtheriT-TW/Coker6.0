using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_OrderDetails_Add_FKShoppingCart_Remove_ProdStock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Details_Prod_Stocks_FK_PSId",
                table: "Order_Details");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Order_Details");

            migrationBuilder.DropColumn(
                name: "Bonus",
                table: "Order_Details");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Order_Details");

            migrationBuilder.RenameColumn(
                name: "FK_PSId",
                table: "Order_Details",
                newName: "FK_SCId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_Details_FK_PSId",
                table: "Order_Details",
                newName: "IX_Order_Details_FK_SCId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Details_ShoppingCarts_FK_SCId",
                table: "Order_Details",
                column: "FK_SCId",
                principalTable: "ShoppingCarts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Details_ShoppingCarts_FK_SCId",
                table: "Order_Details");

            migrationBuilder.RenameColumn(
                name: "FK_SCId",
                table: "Order_Details",
                newName: "FK_PSId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_Details_FK_SCId",
                table: "Order_Details",
                newName: "IX_Order_Details_FK_PSId");

            migrationBuilder.AddColumn<int>(
                name: "Amount",
                table: "Order_Details",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Bonus",
                table: "Order_Details",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Subtotal",
                table: "Order_Details",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Details_Prod_Stocks_FK_PSId",
                table: "Order_Details",
                column: "FK_PSId",
                principalTable: "Prod_Stocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

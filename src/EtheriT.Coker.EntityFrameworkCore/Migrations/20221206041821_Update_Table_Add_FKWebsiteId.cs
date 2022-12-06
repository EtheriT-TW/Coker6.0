using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_Add_FKWebsiteId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Details_Order_Headers_FK_OrderId",
                table: "Order_Details");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Details_Prods_FK_ProductId",
                table: "Order_Details");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_Prod_Specs_FK_S2id",
                table: "ShoppingCarts");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_Prods_FK_Pid",
                table: "ShoppingCarts");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCarts_FK_S2id",
                table: "ShoppingCarts");

            migrationBuilder.RenameColumn(
                name: "FK_Pid",
                table: "ShoppingCarts",
                newName: "FK_PSid");

            migrationBuilder.RenameIndex(
                name: "IX_ShoppingCarts_FK_Pid",
                table: "ShoppingCarts",
                newName: "IX_ShoppingCarts_FK_PSid");

            migrationBuilder.RenameColumn(
                name: "FK_ProductId",
                table: "Order_Details",
                newName: "FK_PSId");

            migrationBuilder.RenameColumn(
                name: "FK_OrderId",
                table: "Order_Details",
                newName: "FK_OId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_Details_FK_ProductId",
                table: "Order_Details",
                newName: "IX_Order_Details_FK_PSId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_Details_FK_OrderId",
                table: "Order_Details",
                newName: "IX_Order_Details_FK_OId");

            migrationBuilder.AddColumn<long>(
                name: "FK_WebsiteId",
                table: "Prods",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "FK_WebsiteId",
                table: "Prod_Spec_Types",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "FK_WebsiteId",
                table: "Order_Headers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Prods_FK_WebsiteId",
                table: "Prods",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Spec_Types_FK_WebsiteId",
                table: "Prod_Spec_Types",
                column: "FK_WebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Details_Order_Headers_FK_OId",
                table: "Order_Details",
                column: "FK_OId",
                principalTable: "Order_Headers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Details_Prod_Stocks_FK_PSId",
                table: "Order_Details",
                column: "FK_PSId",
                principalTable: "Prod_Stocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prod_Spec_Types_Websites_FK_WebsiteId",
                table: "Prod_Spec_Types",
                column: "FK_WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prods_Websites_FK_WebsiteId",
                table: "Prods",
                column: "FK_WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_Prod_Stocks_FK_PSid",
                table: "ShoppingCarts",
                column: "FK_PSid",
                principalTable: "Prod_Stocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Details_Order_Headers_FK_OId",
                table: "Order_Details");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Details_Prod_Stocks_FK_PSId",
                table: "Order_Details");

            migrationBuilder.DropForeignKey(
                name: "FK_Prod_Spec_Types_Websites_FK_WebsiteId",
                table: "Prod_Spec_Types");

            migrationBuilder.DropForeignKey(
                name: "FK_Prods_Websites_FK_WebsiteId",
                table: "Prods");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_Prod_Stocks_FK_PSid",
                table: "ShoppingCarts");

            migrationBuilder.DropIndex(
                name: "IX_Prods_FK_WebsiteId",
                table: "Prods");

            migrationBuilder.DropIndex(
                name: "IX_Prod_Spec_Types_FK_WebsiteId",
                table: "Prod_Spec_Types");

            migrationBuilder.DropColumn(
                name: "FK_WebsiteId",
                table: "Prods");

            migrationBuilder.DropColumn(
                name: "FK_WebsiteId",
                table: "Prod_Spec_Types");

            migrationBuilder.DropColumn(
                name: "FK_WebsiteId",
                table: "Order_Headers");

            migrationBuilder.RenameColumn(
                name: "FK_PSid",
                table: "ShoppingCarts",
                newName: "FK_Pid");

            migrationBuilder.RenameIndex(
                name: "IX_ShoppingCarts_FK_PSid",
                table: "ShoppingCarts",
                newName: "IX_ShoppingCarts_FK_Pid");

            migrationBuilder.RenameColumn(
                name: "FK_PSId",
                table: "Order_Details",
                newName: "FK_ProductId");

            migrationBuilder.RenameColumn(
                name: "FK_OId",
                table: "Order_Details",
                newName: "FK_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_Details_FK_PSId",
                table: "Order_Details",
                newName: "IX_Order_Details_FK_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_Details_FK_OId",
                table: "Order_Details",
                newName: "IX_Order_Details_FK_OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_FK_S2id",
                table: "ShoppingCarts",
                column: "FK_S2id");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Details_Order_Headers_FK_OrderId",
                table: "Order_Details",
                column: "FK_OrderId",
                principalTable: "Order_Headers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Details_Prods_FK_ProductId",
                table: "Order_Details",
                column: "FK_ProductId",
                principalTable: "Prods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_Prod_Specs_FK_S2id",
                table: "ShoppingCarts",
                column: "FK_S2id",
                principalTable: "Prod_Specs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_Prods_FK_Pid",
                table: "ShoppingCarts",
                column: "FK_Pid",
                principalTable: "Prods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

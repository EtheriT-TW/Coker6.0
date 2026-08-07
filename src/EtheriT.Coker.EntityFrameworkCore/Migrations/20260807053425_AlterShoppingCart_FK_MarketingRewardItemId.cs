using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AlterShoppingCart_FK_MarketingRewardItemId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FK_MarketingRewardItemId",
                table: "ShoppingCarts",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_FK_MarketingRewardItemId",
                table: "ShoppingCarts",
                column: "FK_MarketingRewardItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_MarketingRewardItems_FK_MarketingRewardItemId",
                table: "ShoppingCarts",
                column: "FK_MarketingRewardItemId",
                principalTable: "MarketingRewardItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_MarketingRewardItems_FK_MarketingRewardItemId",
                table: "ShoppingCarts");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCarts_FK_MarketingRewardItemId",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "FK_MarketingRewardItemId",
                table: "ShoppingCarts");
        }
    }
}

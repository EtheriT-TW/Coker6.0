using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_ShoppingCart_LogisticSubTypeCVSStoreName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CVSStoreName",
                table: "ShoppingCarts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogisticsSubType",
                table: "ShoppingCarts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CVSStoreName",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "LogisticsSubType",
                table: "ShoppingCarts");
        }
    }
}

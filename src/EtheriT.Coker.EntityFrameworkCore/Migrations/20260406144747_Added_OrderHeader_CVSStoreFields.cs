using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Added_OrderHeader_CVSStoreFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CVSAddress",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVSOutSide",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVSStoreName",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVSTelephone",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CVSAddress",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "CVSOutSide",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "CVSStoreName",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "CVSTelephone",
                table: "Order_Headers");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_Prod_RemovePrice_ProdStock_AddPrice_AddMinQty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Prods");

            migrationBuilder.AddColumn<int>(
                name: "Min_Qty",
                table: "Prod_Stocks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "Prod_Stocks",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 1L,
                column: "Price",
                value: 28000.0);

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 2L,
                column: "Price",
                value: 9500.0);

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 3L,
                column: "Price",
                value: 13000.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Min_Qty",
                table: "Prod_Stocks");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Prod_Stocks");

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "Prods",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 1L,
                column: "Price",
                value: 28000.0);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 2L,
                column: "Price",
                value: 9500.0);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 3L,
                column: "Price",
                value: 13000.0);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddVisibleToProdStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Visible",
                table: "Prod_Stocks",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 1L,
                column: "Visible",
                value: true);

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 2L,
                column: "Visible",
                value: true);

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 3L,
                column: "Visible",
                value: true);

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 4L,
                column: "Visible",
                value: true);

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 5L,
                column: "Visible",
                value: true);

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 6L,
                column: "Visible",
                value: true);

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 7L,
                column: "Visible",
                value: true);

            migrationBuilder.UpdateData(
                table: "Prod_Stocks",
                keyColumn: "Id",
                keyValue: 8L,
                column: "Visible",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Visible",
                table: "Prod_Stocks");
        }
    }
}

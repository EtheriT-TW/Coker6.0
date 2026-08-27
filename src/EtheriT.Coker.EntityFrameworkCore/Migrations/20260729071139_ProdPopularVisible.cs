using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class ProdPopularVisible : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PopularVisible",
                table: "Prods",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 1L,
                column: "PopularVisible",
                value: true);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 2L,
                column: "PopularVisible",
                value: true);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 3L,
                column: "PopularVisible",
                value: true);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 4L,
                column: "PopularVisible",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PopularVisible",
                table: "Prods");
        }
    }
}

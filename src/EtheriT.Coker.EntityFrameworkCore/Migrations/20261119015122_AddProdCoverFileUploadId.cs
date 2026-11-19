using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddProdCoverFileUploadId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CoverFileUploadId",
                table: "Prods",
                type: "bigint",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CoverFileUploadId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CoverFileUploadId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CoverFileUploadId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Prods",
                keyColumn: "Id",
                keyValue: 4L,
                column: "CoverFileUploadId",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverFileUploadId",
                table: "Prods");
        }
    }
}

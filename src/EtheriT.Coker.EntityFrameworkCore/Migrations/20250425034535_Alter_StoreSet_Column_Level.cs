using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Alter_StoreSet_Column_Level : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "StoreSetItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "StoreSet",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 1L,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 2L,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 3L,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 4L,
                column: "Level",
                value: 3);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 5L,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 6L,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 7L,
                column: "Level",
                value: 3);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 8L,
                column: "Level",
                value: 2);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 9L,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 10L,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 11L,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 12L,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 13L,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 14L,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 1L,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 2L,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 3L,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 4L,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 5L,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 6L,
                column: "Level",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 7L,
                column: "Level",
                value: 3);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 8L,
                column: "Level",
                value: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "StoreSetItems");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "StoreSet");
        }
    }
}

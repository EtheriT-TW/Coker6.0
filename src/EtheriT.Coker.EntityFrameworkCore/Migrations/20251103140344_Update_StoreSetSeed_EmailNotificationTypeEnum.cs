using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Update_StoreSetSeed_EmailNotificationTypeEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 22L,
                column: "pattern",
                value: "");

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 11L,
                column: "Key",
                value: "0");

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 12L,
                column: "Key",
                value: "1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 22L,
                column: "pattern",
                value: "(?=[a-z]{2}-?[A-Z]{0,2},?)+");

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 11L,
                column: "Key",
                value: "Detailed");

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 12L,
                column: "Key",
                value: "Simple");
        }
    }
}

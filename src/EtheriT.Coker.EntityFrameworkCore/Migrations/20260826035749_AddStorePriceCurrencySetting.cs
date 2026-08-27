using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddStorePriceCurrencySetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DefaultValue", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "LastModificationTime", "LastModifierUserId", "Level", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 33L, new DateTime(2026, 8, 26, 12, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, "TWD", null, null, 2L, null, null, null, "E001", "priceCurrency", 3, "商品價格與搜尋引擎結構化資料使用的 ISO 4217 幣別", "商品幣別", "^[A-Z]{3}$", 5 });

            migrationBuilder.InsertData(
                table: "StoreSetItems",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetId", "IsDefault", "Key", "LastModificationTime", "LastModifierUserId", "Level", "Value" },
                values: new object[,]
                {
                    { 20L, new DateTime(2026, 8, 26, 12, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 33L, true, "TWD", null, null, null, "新臺幣（TWD）" },
                    { 21L, new DateTime(2026, 8, 26, 12, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 33L, false, "USD", null, null, null, "美元（USD）" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 20L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 21L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 33L);
        }
    }
}

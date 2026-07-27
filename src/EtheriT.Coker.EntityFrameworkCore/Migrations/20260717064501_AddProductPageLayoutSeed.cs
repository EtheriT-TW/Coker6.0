using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddProductPageLayoutSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DefaultValue", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "LastModificationTime", "LastModifierUserId", "Level", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 29L, new DateTime(2026, 7, 17, 11, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 2L, null, null, null, "E001", "ProductPageLayout", null, "設定商品頁面的顯示版型", "商品頁版型", "", 3 });

            migrationBuilder.InsertData(
                table: "StoreSetItems",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetId", "IsDefault", "Key", "LastModificationTime", "LastModifierUserId", "Level", "Value" },
                values: new object[,]
                {
                    { 18L, new DateTime(2026, 7, 17, 11, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 29L, true, "Layout_1", null, null, null, "版型一" },
                    { 19L, new DateTime(2026, 7, 17, 11, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 29L, false, "Layout_2", null, null, null, "版型二" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 18L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 19L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 29L);
        }
    }
}

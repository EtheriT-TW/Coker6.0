using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddProductRecommendationsStoreSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DefaultValue", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "LastModificationTime", "LastModifierUserId", "Level", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 35L, new DateTime(2026, 10, 30, 12, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, "Disabled", null, null, 2L, null, null, null, "E001", "ProductRecommendations", 20, "於商品頁「上一則／下一則」上方顯示依標籤推薦的商品", "推薦商品模組", "", 3 });

            migrationBuilder.InsertData(
                table: "StoreSetItems",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetId", "IsDefault", "Key", "LastModificationTime", "LastModifierUserId", "Level", "Value" },
                values: new object[,]
                {
                    { 29L, new DateTime(2026, 10, 30, 12, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 35L, true, "Disabled", null, null, null, "關閉" },
                    { 30L, new DateTime(2026, 10, 30, 12, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 35L, false, "Enabled", null, null, null, "開啟" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 29L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 30L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 35L);
        }
    }
}

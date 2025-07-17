using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Alter_Seed_StoreSet_PriceOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "LastModificationTime", "LastModifierUserId", "Level", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 21L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, null, null, null, "B001", "priceOrder", 3, "商品預設顯示的金額", "價格顯示順序", "", 5 });

            migrationBuilder.InsertData(
                table: "StoreSetItems",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetId", "Key", "LastModificationTime", "LastModifierUserId", "Level", "Value" },
                values: new object[,]
                {
                    { 9L, new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 21L, "HtoL", null, null, 1, "由高至低" },
                    { 10L, new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 21L, "LtoH", null, null, 1, "由低至高" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 21L);
        }
    }
}

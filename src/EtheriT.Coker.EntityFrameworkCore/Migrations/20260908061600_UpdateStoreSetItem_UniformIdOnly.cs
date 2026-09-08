using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStoreSetItem_UniformIdOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 24L,
                columns: new[] { "memo", "name" },
                values: new object[] { "設定前台結帳時提供的發票資料填寫方式。", "發票開立方式" });

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 15L,
                columns: new[] { "IsDefault", "Value" },
                values: new object[] { true, "填寫完整發票資料" });

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 16L,
                column: "Value",
                value: "不開立發票");

            migrationBuilder.InsertData(
                table: "StoreSetItems",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetId", "IsDefault", "Key", "LastModificationTime", "LastModifierUserId", "Level", "Value" },
                values: new object[] { 28L, new DateTime(2026, 9, 8, 12, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 24L, false, "UniformIdOnly", null, null, null, "僅填寫統一編號" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 28L);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 24L,
                columns: new[] { "memo", "name" },
                values: new object[] { "網站是否需要開立發票", "開立發票" });

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 15L,
                columns: new[] { "IsDefault", "Value" },
                values: new object[] { false, "是" });

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 16L,
                column: "Value",
                value: "否");
        }
    }
}

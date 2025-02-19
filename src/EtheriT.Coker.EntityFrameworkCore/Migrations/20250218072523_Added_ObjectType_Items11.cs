using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_ObjectType_Items11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 3L,
                column: "Title",
                value: "編排樣式");

            migrationBuilder.UpdateData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 8L,
                columns: new[] { "CreationTime", "Title" },
                values: new object[] { new DateTime(2024, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), "標題設計" });

            migrationBuilder.UpdateData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 12L,
                columns: new[] { "CreationTime", "Title" },
                values: new object[] { new DateTime(2024, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), "廣告(加購項目)" });

            migrationBuilder.UpdateData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 99L,
                column: "Title",
                value: "小工具");

            migrationBuilder.InsertData(
                table: "ObjectTypes",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "SerNo", "Title" },
                values: new object[,]
                {
                    { 7L, new DateTime(2024, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "廣告Banner" },
                    { 9L, new DateTime(2024, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "按鈕設計" },
                    { 10L, new DateTime(2024, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "目錄" },
                    { 11L, new DateTime(2024, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, null, null, 500, "進階" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 11L);

            migrationBuilder.UpdateData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 3L,
                column: "Title",
                value: "編排");

            migrationBuilder.UpdateData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 8L,
                columns: new[] { "CreationTime", "Title" },
                values: new object[] { new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), "進入廣告" });

            migrationBuilder.UpdateData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 12L,
                columns: new[] { "CreationTime", "Title" },
                values: new object[] { new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), "浮動廣告" });

            migrationBuilder.UpdateData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 99L,
                column: "Title",
                value: "更多");
        }
    }
}

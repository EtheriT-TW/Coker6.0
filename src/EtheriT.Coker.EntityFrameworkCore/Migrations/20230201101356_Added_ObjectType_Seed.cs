using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_ObjectType_Seed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ObjectTypes",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "IsDeleted", "LastModificationTime", "LastModifierUserId", "SerNo", "Title" },
                values: new object[,]
                {
                    { 1L, new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, false, null, null, 500, "目錄" },
                    { 2L, new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, false, null, null, 500, "廣告" },
                    { 3L, new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, false, null, null, 500, "編排" },
                    { 8L, new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, false, null, null, 500, "進入廣告" },
                    { 12L, new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, false, null, null, 500, "浮動廣告" },
                    { 99L, new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, false, null, null, 500, "更多" },
                    { 999L, new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 2L, null, null, false, null, null, 500, "自訂" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 12L);

            migrationBuilder.DeleteData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 99L);

            migrationBuilder.DeleteData(
                table: "ObjectTypes",
                keyColumn: "Id",
                keyValue: 999L);
        }
    }
}

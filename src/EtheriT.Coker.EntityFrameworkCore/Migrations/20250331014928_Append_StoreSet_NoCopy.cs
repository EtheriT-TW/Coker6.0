using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Append_StoreSet_NoCopy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "StoreSetGroup",
                keyColumn: "Id",
                keyValue: 4L,
                column: "Title",
                value: "版型設定");

            migrationBuilder.InsertData(
                table: "StoreSetGroup",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "Description", "Image", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[] { 5L, new DateTime(2025, 3, 28, 18, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, "", "", null, null, "其他設定" });

            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "LastModificationTime", "LastModifierUserId", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 14L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 5L, null, null, "Y001", "NoCopy", 14, "右鍵鎖定，文字圖片禁止圈選", "鎖右鍵", "", 4 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 14L);

            migrationBuilder.DeleteData(
                table: "StoreSetGroup",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.UpdateData(
                table: "StoreSetGroup",
                keyColumn: "Id",
                keyValue: 4L,
                column: "Title",
                value: "框架設定");
        }
    }
}

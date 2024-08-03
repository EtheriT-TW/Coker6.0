using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_StoreSet_Data_StoreBuyState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459));

            migrationBuilder.InsertData(
                table: "StoreSetGroup",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "Description", "Image", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[] { 2L, new DateTime(2024, 7, 23, 14, 26, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, "", "", false, null, null, "商店設定" });

            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "IsDeleted", "LastModificationTime", "LastModifierUserId", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 3L, new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, "E001", "storeBuyState", 50, "請選擇購物形式", "商品販售設定", "", 5 });

            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "IsDeleted", "LastModificationTime", "LastModifierUserId", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 4L, new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, "E001", "storeMemo", 300, "可以輸入一段話，在結帳的時候對客戶做一些小提醒。", "結帳備註", "", 2 });

            migrationBuilder.InsertData(
                table: "StoreSetItems",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetId", "IsDeleted", "Key", "LastModificationTime", "LastModifierUserId", "Value" },
                values: new object[,]
                {
                    { 5L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 3L, false, "noPayNoShow", null, null, "不開放購物且不顯示商品售價" },
                    { 6L, new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 3L, false, "noPay", null, null, "不開放購物但顯示商品售價" },
                    { 7L, new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 3L, false, "menberPay", null, null, "限制僅會員購物" },
                    { 8L, new DateTime(2024, 7, 23, 14, 38, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 3L, false, "Pay", null, null, "開放購物" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "StoreSetGroup",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2023, 2, 1, 18, 8, 0, 0, DateTimeKind.Local).AddTicks(1459));
        }
    }
}

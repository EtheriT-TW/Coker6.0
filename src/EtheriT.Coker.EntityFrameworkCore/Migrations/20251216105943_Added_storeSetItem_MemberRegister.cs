using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Added_storeSetItem_MemberRegister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "StoreSetItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "StoreSetGroup",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "Description", "Image", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[] { 7L, new DateTime(2025, 12, 16, 17, 7, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, "", "", null, null, "會員設定" });

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 1L,
                column: "IsDefault",
                value: false);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 2L,
                column: "IsDefault",
                value: false);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 3L,
                column: "IsDefault",
                value: false);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 4L,
                column: "IsDefault",
                value: false);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 5L,
                column: "IsDefault",
                value: false);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 6L,
                column: "IsDefault",
                value: false);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 7L,
                column: "IsDefault",
                value: false);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 8L,
                column: "IsDefault",
                value: false);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 9L,
                column: "IsDefault",
                value: false);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 10L,
                column: "IsDefault",
                value: false);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 11L,
                column: "IsDefault",
                value: false);

            migrationBuilder.UpdateData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 12L,
                column: "IsDefault",
                value: false);

            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "LastModificationTime", "LastModifierUserId", "Level", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 23L, new DateTime(2025, 12, 16, 6, 0, 0, 0, DateTimeKind.Local), 1L, null, null, 7L, null, null, null, "M001", "MemberRegister", null, "是否開放註冊，若關閉註冊僅可在會員清單新增。", "開放註冊", "", 5 });

            migrationBuilder.InsertData(
                table: "StoreSetItems",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetId", "IsDefault", "Key", "LastModificationTime", "LastModifierUserId", "Level", "Value" },
                values: new object[,]
                {
                    { 13L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 23L, true, "1", null, null, null, "開放註冊" },
                    { 14L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 23L, false, "3", null, null, null, "關閉註冊" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 13L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 14L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 23L);

            migrationBuilder.DeleteData(
                table: "StoreSetGroup",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "StoreSetItems");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Update_StoreSetSeed_EmailNotificationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "LastModificationTime", "LastModifierUserId", "Level", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 22L, new DateTime(2024, 12, 5, 6, 0, 0, 0, DateTimeKind.Local), 1L, null, null, 3L, null, null, null, "S001", "EmailNotificationType", 50, "請選擇信件寄送方式", "信件寄送方式", "(?=[a-z]{2}-?[A-Z]{0,2},?)+", 3 });

            migrationBuilder.InsertData(
                table: "StoreSetItems",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetId", "Key", "LastModificationTime", "LastModifierUserId", "Level", "Value" },
                values: new object[,]
                {
                    { 11L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 22L, "Detailed", null, null, null, "寄送完整表單" },
                    { 12L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 22L, "Simple", null, null, null, "簡易通知" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 12L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 22L);
        }
    }
}

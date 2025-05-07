using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Insert_BonusSeting_Seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StoreSetGroup",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "Description", "Image", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[] { 6L, new DateTime(2025, 5, 7, 17, 7, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, "", "", null, null, "紅利設定" });

            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "LastModificationTime", "LastModifierUserId", "Level", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[,]
                {
                    { 15L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 6L, null, null, null, "B001", "SignupBonusPoints", 8, "加入會員贈送紅利點數", "迎新禮", "", 8 },
                    { 16L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 6L, null, null, null, "B001", "MinOrderForRedemption", 6, "單筆訂單消費滿足多少可使用紅利扣抵金額", "紅利扣抵條件", "", 8 },
                    { 17L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 6L, null, null, null, "B001", "MaxRedemptionPercent", 2, "單筆訂單抵扣%數上限", "最高抵扣%", "", 8 },
                    { 18L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 6L, null, null, null, "B001", "MinOrderForEarnPoints", 8, "消費滿額多少金額贈送紅利回饋金", "消費條件", "", 8 },
                    { 19L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 6L, null, null, null, "B001", "RewardRatePercent", 2, "消費滿足條件贈送幾%紅利回饋金", "獲得%數紅利", "", 8 },
                    { 20L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 6L, null, null, null, "B001", "RewardPointsExpireDays", 3, "每一筆紅利的有效天數", "有效天數", "", 8 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 15L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 16L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 17L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 18L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 19L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 20L);

            migrationBuilder.DeleteData(
                table: "StoreSetGroup",
                keyColumn: "Id",
                keyValue: 6L);
        }
    }
}

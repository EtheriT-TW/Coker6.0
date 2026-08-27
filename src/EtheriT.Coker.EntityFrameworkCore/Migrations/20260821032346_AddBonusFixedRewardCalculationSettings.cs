using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddBonusFixedRewardCalculationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DefaultValue", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "LastModificationTime", "LastModifierUserId", "Level", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[,]
                {
                    { 30L, new DateTime(2026, 8, 21, 10, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, "Percent", null, null, 6L, null, null, null, "B001", "RewardCalculationType", 20, "設定消費滿額後依百分比或固定點數贈送紅利", "紅利回饋方式", "", 5 },
                    { 31L, new DateTime(2026, 8, 21, 10, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, 6L, null, null, null, "B001", "RewardFixedPoints", 8, "消費滿足條件後贈送的固定紅利點數", "固定回饋點數", "", 8 },
                    { 32L, new DateTime(2026, 8, 21, 10, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, "True", null, null, 6L, null, null, null, "B001", "RewardFixedPointsCumulative", 5, "固定點數是否按消費門檻倍數累計贈送", "固定點數累計贈送", "", 4 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 30L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 31L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 32L);
        }
    }
}

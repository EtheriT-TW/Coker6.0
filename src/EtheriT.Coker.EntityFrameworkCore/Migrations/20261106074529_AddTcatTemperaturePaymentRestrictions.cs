using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddTcatTemperaturePaymentRestrictions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "LogisticsType_Payments",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_LogisticsSettingId", "FK_PaymentTypeId", "IsEnabled", "LastModificationTime", "LastModifierUserId", "OverrideMaxAmount", "OverrideMinAmount", "ShippingType" },
                values: new object[,]
                {
                    { -1019L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 19 },
                    { -1018L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 18 },
                    { -819L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 19 },
                    { -818L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 18 },
                    { -719L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 19 },
                    { -718L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 18 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1019L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1018L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -819L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -818L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -719L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -718L);
        }
    }
}

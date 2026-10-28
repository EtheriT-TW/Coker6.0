using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddECPayInstallmentAndTwqrPaymentTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "CanRefund", "Code", "CreationTime", "CreatorUserId", "CvsStoreSelectionMode", "DeleterUserId", "DeletionTime", "FK_ThirdPartyId", "Icons", "LastModificationTime", "LastModifierUserId", "MaxAmount", "MinAmount", "RefundWorkDay", "RepayAfterMinutes", "SerNo", "ThirdPartyId", "Title", "Used" },
                values: new object[,]
                {
                    { 30L, false, "ECPayFlexibleInstallment_30", new DateTime(2026, 10, 22, 0, 0, 0, 0, DateTimeKind.Local), 1L, 0, null, null, 4L, "", null, null, 199999m, 6m, -1, 10, 26, null, "永豐30期", false },
                    { 31L, false, "ECPayURICH", new DateTime(2026, 10, 22, 0, 0, 0, 0, DateTimeKind.Local), 1L, 0, null, null, 4L, "", null, null, 199999m, 6m, -1, 10, 27, null, "URICH 裕富無卡分期", false },
                    { 32L, false, "ECPayZINGALA", new DateTime(2026, 10, 22, 0, 0, 0, 0, DateTimeKind.Local), 1L, 0, null, null, 4L, "", null, null, 199999m, 6m, -1, 10, 28, null, "ZINGALA 中租銀角零卡", false },
                    { 33L, false, "ECPayTWQR", new DateTime(2026, 10, 22, 0, 0, 0, 0, DateTimeKind.Local), 1L, 0, null, null, 4L, "", null, null, 199999m, 6m, -1, 10, 29, null, "TWQR", false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 30L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 31L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 32L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 33L);
        }
    }
}

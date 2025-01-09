using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Seed_ECPay : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_ThirdPartyId", "LastModificationTime", "LastModifierUserId", "SerNo", "ThirdPartyId", "Title", "Used" },
                values: new object[,]
                {
                    { 16L, "ECPayCreditCard", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, 14, null, "信用卡付款", false },
                    { 17L, "ECPayUnionPay", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, 15, null, "信用卡付款(銀聯卡)", false },
                    { 18L, "ECPayCreditInstallment", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, 16, null, "信用卡分期付款", false },
                    { 19L, "ECPayATM", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, 17, null, "ATM(虛擬帳戶)", false },
                    { 20L, "ECPayBarcode", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, 18, null, "超商條碼付款", false },
                    { 21L, "ECPayCVS", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, 19, null, "超商代碼付款", false },
                    { 22L, "ECPayApplePay", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, 20, null, "ApplePay", false }
                });

            migrationBuilder.InsertData(
                table: "ThirdParties",
                columns: new[] { "Id", "AuditUrl", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "MaxPay", "PaymentUrl", "RefundUrl", "Title", "TokenUrl", "ser_no" },
                values: new object[] { 4L, null, new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, null, null, null, null, null, "綠界支付", null, 500 });

            migrationBuilder.InsertData(
                table: "ThirdPartyKeypairs",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_TPid", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[,]
                {
                    { 9L, "MerchantID", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, "商店代號" },
                    { 10L, "PlatformID ", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, "平台代號(非專案合作請留空)" },
                    { 11L, "HashKey ", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, "HashKey" },
                    { 12L, "HashIV ", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, "HashIV" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 16L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 17L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 18L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 19L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 20L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 21L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 22L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 12L);

            migrationBuilder.DeleteData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 4L);
        }
    }
}

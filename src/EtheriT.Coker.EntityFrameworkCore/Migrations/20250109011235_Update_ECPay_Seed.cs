using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_ECPay_Seed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PromptText",
                table: "ThirdPartyKeypairs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 18L,
                columns: new[] { "Code", "Title" },
                values: new object[] { "ECPayCreditInstallment_3", "信用卡分期付款3期" });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 19L,
                columns: new[] { "Code", "Title" },
                values: new object[] { "ECPayCreditInstallment_6", "信用卡分期付款6期" });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 20L,
                columns: new[] { "Code", "Title" },
                values: new object[] { "ECPayCreditInstallment_12", "信用卡分期付款12期" });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 21L,
                columns: new[] { "Code", "Title" },
                values: new object[] { "ECPayATM", "ATM(虛擬帳戶)" });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 22L,
                columns: new[] { "Code", "Title" },
                values: new object[] { "ECPayBarcode", "超商條碼付款" });

            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_ThirdPartyId", "LastModificationTime", "LastModifierUserId", "SerNo", "ThirdPartyId", "Title", "Used" },
                values: new object[,]
                {
                    { 23L, "ECPayCVS_OK", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, 21, null, "超商代碼付款(OK)", false },
                    { 24L, "ECPayCVS_FAMILY", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, 22, null, "超商代碼付款(全家)", false },
                    { 25L, "ECPayCVS_HILIFE", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, 23, null, "超商代碼付款(萊爾富)", false },
                    { 26L, "ECPayCVS_IBON", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, 24, null, "超商代碼付款(7-11)", false },
                    { 27L, "ECPayApplePay", new DateTime(2024, 11, 21, 14, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, 25, null, "ApplePay", false }
                });

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 6L,
                columns: new[] { "Code", "PromptText" },
                values: new object[] { "expire_days", "※預設為5天，最短1天，最長可設定為5天，超過一律以5天計算" });

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 10L,
                columns: new[] { "PromptText", "Title" },
                values: new object[] { "※非專案合作請留空", "平台代號" });

            migrationBuilder.InsertData(
                table: "ThirdPartyKeypairs",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_TPid", "LastModificationTime", "LastModifierUserId", "PromptText", "Title" },
                values: new object[,]
                {
                    { 13L, "ExpireDate", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, "※預設為3天，最短1天，最長可設定為60天，超過一律以60天計算", "ATM允許繳費有效天數" },
                    { 14L, "StoreExpireDate_Barcode", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, "※預設為7天，最短1天，最長可設定為30天，超過一律以30天計算", "超商條碼繳費截止時間" },
                    { 15L, "StoreExpireDate_CVS", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 4L, null, null, "※預設為7天，最短1天，最長可設定為30天，超過一律以30天計算", "超商代碼繳費截止時間" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 23L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 24L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 25L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 26L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 27L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 13L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 14L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 15L);

            migrationBuilder.DropColumn(
                name: "PromptText",
                table: "ThirdPartyKeypairs");

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 18L,
                columns: new[] { "Code", "Title" },
                values: new object[] { "ECPayCreditInstallment", "信用卡分期付款" });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 19L,
                columns: new[] { "Code", "Title" },
                values: new object[] { "ECPayATM", "ATM(虛擬帳戶)" });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 20L,
                columns: new[] { "Code", "Title" },
                values: new object[] { "ECPayBarcode", "超商條碼付款" });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 21L,
                columns: new[] { "Code", "Title" },
                values: new object[] { "ECPayCVS", "超商代碼付款" });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 22L,
                columns: new[] { "Code", "Title" },
                values: new object[] { "ECPayApplePay", "ApplePay" });

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 6L,
                column: "Code",
                value: "expire_day2");

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 10L,
                column: "Title",
                value: "平台代號(非專案合作請留空)");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Table_PaymentTypesValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTypes_ThirdParties_ThirdPartyId",
                table: "PaymentTypes");

            migrationBuilder.DropColumn(
                name: "ThirdKey",
                table: "PaymentTypes");

            migrationBuilder.AlterColumn<long>(
                name: "ThirdPartyId",
                table: "PaymentTypes",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateTable(
                name: "PaymentTypesValues",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Used = table.Column<bool>(type: "bit", nullable: false),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_PaymentTypesId = table.Column<long>(type: "bigint", nullable: false),
                    websiteId = table.Column<long>(type: "bigint", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTypesValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTypesValues_PaymentTypes_FK_PaymentTypesId",
                        column: x => x.FK_PaymentTypesId,
                        principalTable: "PaymentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentTypesValues_Websites_websiteId",
                        column: x => x.websiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_ThirdPartyId", "IsDeleted", "LastModificationTime", "LastModifierUserId", "SerNo", "ThirdPartyId", "Title", "Used" },
                values: new object[,]
                {
                    { 1L, null, new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 1L, false, null, null, 1, null, "atm", false },
                    { 2L, "PchomePayCARD", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, 500, null, "信用卡付款", false },
                    { 3L, "PchomePayATM", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, 500, null, "ATM付款", false },
                    { 4L, "PchomePayPI", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, 500, null, "PI錢包付款", false },
                    { 5L, "PchomePayACCT", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, 500, null, "支付連餘額付款", false },
                    { 6L, "PchomePayEACH", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, 500, null, "支付連銀行支付付款", false },
                    { 7L, "PchomePayEACH", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, 500, null, "7-11貨到付款", false },
                    { 8L, "PCHomeIPLFM", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, 500, null, "全家貨到付款", false },
                    { 9L, "PCHomeIPLOK", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, 500, null, "OK貨到付款", false },
                    { 10L, "PCHomeIPLHL", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, 500, null, "萊爾富貨到付款", false },
                    { 11L, "PchomePayInstallment3", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, 500, null, "線上刷卡3期分期付款", false },
                    { 12L, "PchomePayInstallment6", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, 500, null, "線上刷卡6期分期付款", false },
                    { 13L, "PchomePayInstallment12", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, 500, null, "線上刷卡12期分期付款", false },
                    { 14L, "LinePay", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 3L, false, null, null, 500, null, "LINEPay", false }
                });

            migrationBuilder.InsertData(
                table: "ThirdParties",
                columns: new[] { "Id", "AuditUrl", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "IsDeleted", "LastModificationTime", "LastModifierUserId", "MaxPay", "PaymentUrl", "RefundUrl", "Title", "TokenUrl", "ser_no" },
                values: new object[,]
                {
                    { 2L, null, new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, false, null, null, null, null, null, "支付連", null, 500 },
                    { 3L, null, new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, false, null, null, null, null, null, "LINE Pay", null, 500 }
                });

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 3L,
                column: "Title",
                value: "戶名");

            migrationBuilder.InsertData(
                table: "ThirdPartyKeypairs",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_TPid", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[,]
                {
                    { 4L, "account", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, "PchomePayAppId" },
                    { 5L, "code1", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, "PchomePaySecre" },
                    { 6L, "expire_day2", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, null, null, "允許繳費有效天數" },
                    { 7L, "account", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 3L, false, null, null, "Channel ID" },
                    { 8L, "code1", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 3L, false, null, null, "Channel Secret Key" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypesValues_FK_PaymentTypesId",
                table: "PaymentTypesValues",
                column: "FK_PaymentTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypesValues_websiteId",
                table: "PaymentTypesValues",
                column: "websiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTypes_ThirdParties_ThirdPartyId",
                table: "PaymentTypes",
                column: "ThirdPartyId",
                principalTable: "ThirdParties",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTypes_ThirdParties_ThirdPartyId",
                table: "PaymentTypes");

            migrationBuilder.DropTable(
                name: "PaymentTypesValues");

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 12L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 13L);

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 14L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.AlterColumn<long>(
                name: "ThirdPartyId",
                table: "PaymentTypes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThirdKey",
                table: "PaymentTypes",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 3L,
                column: "Title",
                value: "戶名 ");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTypes_ThirdParties_ThirdPartyId",
                table: "PaymentTypes",
                column: "ThirdPartyId",
                principalTable: "ThirdParties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

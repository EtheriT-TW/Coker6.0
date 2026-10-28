using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class RemoveECPayFlexibleInstallment_30 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        IF EXISTS (
            SELECT 1
            FROM [Order_Headers]
            WHERE [Payment] = 30
        )
        BEGIN
            THROW 50000, N'仍有訂單使用永豐30期，停止移除。', 1;
        END;

        DELETE FROM [PaymentTypesValues]
        WHERE [FK_PaymentTypesId] = 30;

        DELETE FROM [LogisticsType_Payments]
        WHERE [FK_PaymentTypeId] = 30;
    ");

            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 30L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "CanRefund", "Code", "CreationTime", "CreatorUserId", "CvsStoreSelectionMode", "DeleterUserId", "DeletionTime", "FK_ThirdPartyId", "Icons", "LastModificationTime", "LastModifierUserId", "MaxAmount", "MinAmount", "RefundWorkDay", "RepayAfterMinutes", "SerNo", "ThirdPartyId", "Title", "Used" },
                values: new object[] { 30L, false, "ECPayFlexibleInstallment_30", new DateTime(2026, 10, 22, 0, 0, 0, 0, DateTimeKind.Local), 1L, 0, null, null, 4L, "", null, null, 199999m, 6m, -1, 10, 26, null, "永豐30期", false });
        }
    }
}

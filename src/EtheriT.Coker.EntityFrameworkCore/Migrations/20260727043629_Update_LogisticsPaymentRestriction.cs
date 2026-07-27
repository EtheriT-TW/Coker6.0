using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Update_LogisticsPaymentRestriction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogisticsType_Payments_PaymentTypes_FK_Pid",
                table: "LogisticsType_Payments");

            migrationBuilder.DropIndex(
                name: "IX_LogisticsType_Payments_FK_Pid",
                table: "LogisticsType_Payments");

            migrationBuilder.DropColumn(
                name: "FK_Pid",
                table: "LogisticsType_Payments");

            migrationBuilder.AlterColumn<decimal>(
                name: "MinAmount",
                table: "PaymentTypes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxAmount",
                table: "PaymentTypes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ShippingType",
                table: "LogisticsType_Payments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<long>(
                name: "FK_LogisticsSettingId",
                table: "LogisticsType_Payments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FK_PaymentTypeId",
                table: "LogisticsType_Payments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "LogisticsType_Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "OverrideMaxAmount",
                table: "LogisticsType_Payments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OverrideMinAmount",
                table: "LogisticsType_Payments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            // SupportCashOnDelivery 只適用於綠界超商物流（LogisticsType 8～15）。
            // 舊欄位為 false 代表該綠界物流設定不支援一般貨到付款。
            // 新規則未設定時預設開放，因此只需搬移 false 的設定。
            migrationBuilder.Sql(
                """
                INSERT INTO [LogisticsType_Payments]
                (
                    [CreationTime],
                    [CreatorUserId],
                    [IsDeleted],
                    [ShippingType],
                    [FK_LogisticsSettingId],
                    [FK_PaymentTypeId],
                    [IsEnabled],
                    [OverrideMinAmount],
                    [OverrideMaxAmount]
                )
                SELECT
                    GETDATE(),
                    1,
                    0,
                    NULL,
                    [logistics].[Id],
                    28,
                    0,
                    NULL,
                    NULL
                FROM [LogisticsSettings] AS [logistics]
                WHERE [logistics].[IsDeleted] = 0
                  AND [logistics].[LogisticsType] BETWEEN 8 AND 15
                  AND [logistics].[SupportCashOnDelivery] = 0
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM [LogisticsType_Payments] AS [restriction]
                      WHERE [restriction].[FK_LogisticsSettingId] = [logistics].[Id]
                        AND [restriction].[FK_PaymentTypeId] = 28
                        AND [restriction].[IsDeleted] = 0
                  );
                """);

            migrationBuilder.InsertData(
                table: "LogisticsType_Payments",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_LogisticsSettingId", "FK_PaymentTypeId", "IsEnabled", "LastModificationTime", "LastModifierUserId", "OverrideMaxAmount", "OverrideMinAmount", "ShippingType" },
                values: new object[,]
                {
                    { -1017L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 17 },
                    { -1016L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 16 },
                    { -1015L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 15 },
                    { -1014L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 14 },
                    { -1013L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 13 },
                    { -1012L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 12 },
                    { -1011L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 11 },
                    { -1010L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 10 },
                    { -1009L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 9 },
                    { -1008L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 8 },
                    { -1006L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 6 },
                    { -1005L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 5 },
                    { -1004L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 4 },
                    { -1003L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 3 },
                    { -1002L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 2 },
                    { -1001L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 10L, false, null, null, null, null, 1 },
                    { -817L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 17 },
                    { -816L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 16 },
                    { -815L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 15 },
                    { -814L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 14 },
                    { -813L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 13 },
                    { -812L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 12 },
                    { -811L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 11 },
                    { -810L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 10 },
                    { -809L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 9 },
                    { -808L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 8 },
                    { -807L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 7 },
                    { -806L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 6 },
                    { -805L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 5 },
                    { -803L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 3 },
                    { -802L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 2 },
                    { -801L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 8L, false, null, null, null, null, 1 },
                    { -717L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 17 },
                    { -716L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 16 },
                    { -715L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 15 },
                    { -714L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 14 },
                    { -713L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 13 },
                    { -712L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 12 },
                    { -711L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 11 },
                    { -710L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 10 },
                    { -709L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 9 },
                    { -708L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 8 },
                    { -707L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 7 },
                    { -706L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 6 },
                    { -704L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 4 },
                    { -703L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 3 },
                    { -702L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 2 },
                    { -701L, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, 7L, false, null, null, null, null, 1 }
                });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { null, 1m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999m, 30m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 49999m, 1m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999m, 1m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 5L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { null, 1m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 6L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 49999m, 1m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 7L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000m, 65m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 8L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000m, 65m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 9L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000m, 65m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 10L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000m, 65m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 11L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999m, 30m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 12L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999m, 30m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 13L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999m, 30m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 14L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 50000m, 1m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 15L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000m, 25m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 16L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999m, 6m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 17L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999m, 6m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 18L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999m, 6m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 19L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999m, 6m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 20L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999m, 6m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 21L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 49999m, 17m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 22L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000m, 16m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 23L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000m, 31m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 24L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000m, 31m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 25L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000m, 31m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 26L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000m, 31m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 27L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999m, 6m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 28L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { null, 1m });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 29L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { null, 1m });

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsType_Payments_FK_LogisticsSettingId_FK_PaymentTypeId",
                table: "LogisticsType_Payments",
                columns: new[] { "FK_LogisticsSettingId", "FK_PaymentTypeId" },
                unique: true,
                filter: "[FK_LogisticsSettingId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsType_Payments_FK_PaymentTypeId",
                table: "LogisticsType_Payments",
                column: "FK_PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsType_Payments_ShippingType_FK_PaymentTypeId",
                table: "LogisticsType_Payments",
                columns: new[] { "ShippingType", "FK_PaymentTypeId" },
                unique: true,
                filter: "[FK_LogisticsSettingId] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LogisticsType_Payments_RuleScope",
                table: "LogisticsType_Payments",
                sql: "([ShippingType] IS NOT NULL AND [FK_LogisticsSettingId] IS NULL) OR ([ShippingType] IS NULL AND [FK_LogisticsSettingId] IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_LogisticsType_Payments_LogisticsSettings_FK_LogisticsSettingId",
                table: "LogisticsType_Payments",
                column: "FK_LogisticsSettingId",
                principalTable: "LogisticsSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LogisticsType_Payments_PaymentTypes_FK_PaymentTypeId",
                table: "LogisticsType_Payments",
                column: "FK_PaymentTypeId",
                principalTable: "PaymentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogisticsType_Payments_LogisticsSettings_FK_LogisticsSettingId",
                table: "LogisticsType_Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_LogisticsType_Payments_PaymentTypes_FK_PaymentTypeId",
                table: "LogisticsType_Payments");

            migrationBuilder.DropIndex(
                name: "IX_LogisticsType_Payments_FK_LogisticsSettingId_FK_PaymentTypeId",
                table: "LogisticsType_Payments");

            migrationBuilder.DropIndex(
                name: "IX_LogisticsType_Payments_FK_PaymentTypeId",
                table: "LogisticsType_Payments");

            migrationBuilder.DropIndex(
                name: "IX_LogisticsType_Payments_ShippingType_FK_PaymentTypeId",
                table: "LogisticsType_Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LogisticsType_Payments_RuleScope",
                table: "LogisticsType_Payments");

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1017L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1016L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1015L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1014L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1013L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1012L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1011L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1010L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1009L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1008L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1006L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1005L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1004L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1003L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1002L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -1001L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -817L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -816L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -815L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -814L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -813L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -812L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -811L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -810L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -809L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -808L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -807L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -806L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -805L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -803L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -802L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -801L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -717L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -716L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -715L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -714L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -713L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -712L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -711L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -710L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -709L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -708L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -707L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -706L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -704L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -703L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -702L);

            migrationBuilder.DeleteData(
                table: "LogisticsType_Payments",
                keyColumn: "Id",
                keyValue: -701L);

            migrationBuilder.Sql(
                """
                DELETE [restriction]
                FROM [LogisticsType_Payments] AS [restriction]
                INNER JOIN [LogisticsSettings] AS [logistics]
                    ON [logistics].[Id] = [restriction].[FK_LogisticsSettingId]
                WHERE [restriction].[FK_PaymentTypeId] = 28
                  AND [logistics].[LogisticsType] BETWEEN 8 AND 15
                  AND [logistics].[SupportCashOnDelivery] = 0;
                """);

            migrationBuilder.DropColumn(
                name: "FK_LogisticsSettingId",
                table: "LogisticsType_Payments");

            migrationBuilder.DropColumn(
                name: "FK_PaymentTypeId",
                table: "LogisticsType_Payments");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "LogisticsType_Payments");

            migrationBuilder.DropColumn(
                name: "OverrideMaxAmount",
                table: "LogisticsType_Payments");

            migrationBuilder.DropColumn(
                name: "OverrideMinAmount",
                table: "LogisticsType_Payments");

            migrationBuilder.AlterColumn<int>(
                name: "MinAmount",
                table: "PaymentTypes",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "MaxAmount",
                table: "PaymentTypes",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ShippingType",
                table: "LogisticsType_Payments",
                type: "int",
                maxLength: 50,
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FK_Pid",
                table: "LogisticsType_Payments",
                type: "bigint",
                maxLength: 50,
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { null, 1 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999, 30 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 49999, 1 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999, 1 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 5L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { null, 1 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 6L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 49999, 1 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 7L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000, 65 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 8L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000, 65 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 9L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000, 65 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 10L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000, 65 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 11L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999, 30 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 12L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999, 30 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 13L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999, 30 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 14L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 50000, 1 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 15L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000, 25 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 16L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999, 6 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 17L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999, 6 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 18L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999, 6 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 19L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999, 6 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 20L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999, 6 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 21L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 49999, 17 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 22L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000, 16 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 23L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000, 31 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 24L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000, 31 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 25L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000, 31 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 26L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 20000, 31 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 27L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { 199999, 6 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 28L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { null, 1 });

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 29L,
                columns: new[] { "MaxAmount", "MinAmount" },
                values: new object[] { null, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsType_Payments_FK_Pid",
                table: "LogisticsType_Payments",
                column: "FK_Pid");

            migrationBuilder.AddForeignKey(
                name: "FK_LogisticsType_Payments_PaymentTypes_FK_Pid",
                table: "LogisticsType_Payments",
                column: "FK_Pid",
                principalTable: "PaymentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

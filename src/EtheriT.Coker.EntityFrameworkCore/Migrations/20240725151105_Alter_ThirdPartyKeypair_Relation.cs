using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Alter_ThirdPartyKeypair_Relation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Account",
                table: "ThirdParties");

            migrationBuilder.DropColumn(
                name: "Auto_Deposit",
                table: "ThirdParties");

            migrationBuilder.DropColumn(
                name: "Code1",
                table: "ThirdParties");

            migrationBuilder.DropColumn(
                name: "Code2",
                table: "ThirdParties");

            migrationBuilder.DropColumn(
                name: "Expire_Day",
                table: "ThirdParties");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "ThirdParties");

            migrationBuilder.DropColumn(
                name: "ShopID",
                table: "ThirdParties");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ThirdParties");

            migrationBuilder.DropColumn(
                name: "TaxID",
                table: "ThirdParties");

            migrationBuilder.DropColumn(
                name: "Disp_Opt",
                table: "PaymentTypes");

            migrationBuilder.DropColumn(
                name: "Head_column",
                table: "PaymentTypes");

            migrationBuilder.DropColumn(
                name: "ThirdID",
                table: "PaymentTypes");

            migrationBuilder.RenameColumn(
                name: "Ser_No",
                table: "PaymentTypes",
                newName: "SerNo");

            migrationBuilder.AddColumn<long>(
                name: "FK_ThirdPartyId",
                table: "PaymentTypes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ThirdPartyId",
                table: "PaymentTypes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "ThirdPartyKeypairValues",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_ThirdPartyKeypairId = table.Column<long>(type: "bigint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    WebsiteId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_ThirdPartyKeypairValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThirdPartyKeypairValues_ThirdPartyKeypairs_FK_ThirdPartyKeypairId",
                        column: x => x.FK_ThirdPartyKeypairId,
                        principalTable: "ThirdPartyKeypairs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThirdPartyKeypairValues_Websites_WebsiteId",
                        column: x => x.WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 1L,
                column: "pattern",
                value: "^G-\\w+");

            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "IsDeleted", "LastModificationTime", "LastModifierUserId", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 5L, new DateTime(2023, 7, 25, 19, 8, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 1L, false, null, null, "S001", "GTM", 12, "請輸入GOOGLE提供之驗證碼：GTM-xxxxxxx", "Google Tag Manager", "^GTM-\\w+", 1 });

            migrationBuilder.InsertData(
                table: "ThirdParties",
                columns: new[] { "Id", "AuditUrl", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "IsDeleted", "LastModificationTime", "LastModifierUserId", "MaxPay", "PaymentUrl", "RefundUrl", "Title", "TokenUrl", "ser_no" },
                values: new object[] { 1L, null, new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, false, null, null, null, null, null, "轉帳", null, 500 });

            migrationBuilder.InsertData(
                table: "ThirdPartyKeypairs",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_TPid", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[] { 1L, "bankNo", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 1L, false, null, null, "匯款銀行代號" });

            migrationBuilder.InsertData(
                table: "ThirdPartyKeypairs",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_TPid", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[] { 2L, "account", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 1L, false, null, null, "匯款帳號" });

            migrationBuilder.InsertData(
                table: "ThirdPartyKeypairs",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_TPid", "IsDeleted", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[] { 3L, "shopID", new DateTime(2024, 7, 25, 19, 25, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 1L, false, null, null, "戶名 " });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypes_ThirdPartyId",
                table: "PaymentTypes",
                column: "ThirdPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_ThirdPartyKeypairValues_FK_ThirdPartyKeypairId",
                table: "ThirdPartyKeypairValues",
                column: "FK_ThirdPartyKeypairId");

            migrationBuilder.CreateIndex(
                name: "IX_ThirdPartyKeypairValues_WebsiteId",
                table: "ThirdPartyKeypairValues",
                column: "WebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTypes_ThirdParties_ThirdPartyId",
                table: "PaymentTypes",
                column: "ThirdPartyId",
                principalTable: "ThirdParties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTypes_ThirdParties_ThirdPartyId",
                table: "PaymentTypes");

            migrationBuilder.DropTable(
                name: "ThirdPartyKeypairValues");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTypes_ThirdPartyId",
                table: "PaymentTypes");

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "ThirdPartyKeypairs",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "ThirdParties",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DropColumn(
                name: "FK_ThirdPartyId",
                table: "PaymentTypes");

            migrationBuilder.DropColumn(
                name: "ThirdPartyId",
                table: "PaymentTypes");

            migrationBuilder.RenameColumn(
                name: "SerNo",
                table: "PaymentTypes",
                newName: "Ser_No");

            migrationBuilder.AddColumn<string>(
                name: "Account",
                table: "ThirdParties",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Auto_Deposit",
                table: "ThirdParties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Code1",
                table: "ThirdParties",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code2",
                table: "ThirdParties",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Expire_Day",
                table: "ThirdParties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "ThirdParties",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopID",
                table: "ThirdParties",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ThirdParties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TaxID",
                table: "ThirdParties",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Disp_Opt",
                table: "PaymentTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Head_column",
                table: "PaymentTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThirdID",
                table: "PaymentTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 1L,
                column: "pattern",
                value: "^G-\\w");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Add_ProdLogUpdate_Prod_ProdSpec_ProdStock_Token_Use : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogisticsType_PaymentType_Logisticstype_FK_Lid",
                table: "LogisticsType_PaymentType");

            migrationBuilder.DropForeignKey(
                name: "FK_LogisticsType_PaymentType_PaymentType_FK_Pid",
                table: "LogisticsType_PaymentType");

            migrationBuilder.DropForeignKey(
                name: "FK_Prod_Spec_Prod_Spec_Type_FK_Tid",
                table: "Prod_Spec");

            migrationBuilder.DropForeignKey(
                name: "FK_Prod_Stock_Prod_Spec_FK_S2id",
                table: "Prod_Stock");

            migrationBuilder.DropForeignKey(
                name: "FK_ThirdPartyKeypair_ThirdParty_FK_TPid",
                table: "ThirdPartyKeypair");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ThirdPartyKeypair",
                table: "ThirdPartyKeypair");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ThirdParty",
                table: "ThirdParty");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prod_Stock",
                table: "Prod_Stock");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prod_Spec_Type",
                table: "Prod_Spec_Type");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prod_Spec",
                table: "Prod_Spec");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentType",
                table: "PaymentType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LogisticsType_PaymentType",
                table: "LogisticsType_PaymentType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Logisticstype",
                table: "Logisticstype");

            migrationBuilder.RenameTable(
                name: "ThirdPartyKeypair",
                newName: "ThirdPartyKeypairs");

            migrationBuilder.RenameTable(
                name: "ThirdParty",
                newName: "ThirdParties");

            migrationBuilder.RenameTable(
                name: "Prod_Stock",
                newName: "Prod_Stocks");

            migrationBuilder.RenameTable(
                name: "Prod_Spec_Type",
                newName: "Prod_Spec_Types");

            migrationBuilder.RenameTable(
                name: "Prod_Spec",
                newName: "Prod_Specs");

            migrationBuilder.RenameTable(
                name: "PaymentType",
                newName: "PaymentTypes");

            migrationBuilder.RenameTable(
                name: "LogisticsType_PaymentType",
                newName: "LogisticsType_Payments");

            migrationBuilder.RenameTable(
                name: "Logisticstype",
                newName: "Logisticstypes");

            migrationBuilder.RenameIndex(
                name: "IX_ThirdPartyKeypair_FK_TPid",
                table: "ThirdPartyKeypairs",
                newName: "IX_ThirdPartyKeypairs_FK_TPid");

            migrationBuilder.RenameIndex(
                name: "IX_Prod_Stock_FK_S2id",
                table: "Prod_Stocks",
                newName: "IX_Prod_Stocks_FK_S2id");

            migrationBuilder.RenameIndex(
                name: "IX_Prod_Spec_FK_Tid",
                table: "Prod_Specs",
                newName: "IX_Prod_Specs_FK_Tid");

            migrationBuilder.RenameIndex(
                name: "IX_LogisticsType_PaymentType_FK_Pid",
                table: "LogisticsType_Payments",
                newName: "IX_LogisticsType_Payments_FK_Pid");

            migrationBuilder.RenameIndex(
                name: "IX_LogisticsType_PaymentType_FK_Lid",
                table: "LogisticsType_Payments",
                newName: "IX_LogisticsType_Payments_FK_Lid");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Prods",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "Prods",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Introduction",
                table: "Prods",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "Prods",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ThirdPartyKeypairs",
                table: "ThirdPartyKeypairs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ThirdParties",
                table: "ThirdParties",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prod_Stocks",
                table: "Prod_Stocks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prod_Spec_Types",
                table: "Prod_Spec_Types",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prod_Specs",
                table: "Prod_Specs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentTypes",
                table: "PaymentTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LogisticsType_Payments",
                table: "LogisticsType_Payments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Logisticstypes",
                table: "Logisticstypes",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "LogisticsSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PreserveType = table.Column<int>(type: "int", nullable: false),
                    LogisticsType = table.Column<int>(type: "int", nullable: false),
                    FreigntType = table.Column<int>(type: "int", nullable: false),
                    Low_Con = table.Column<int>(type: "int", nullable: true),
                    Dis_Freight = table.Column<int>(type: "int", nullable: true),
                    Set_Default = table.Column<bool>(type: "bit", nullable: false),
                    FreigntAmt2 = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_LogisticsSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prod_Logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_Pid = table.Column<long>(type: "bigint", nullable: false),
                    FK_Uid = table.Column<long>(type: "bigint", nullable: true),
                    FK_Tid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    Db_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Prod_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prod_Logs_Prods_FK_Pid",
                        column: x => x.FK_Pid,
                        principalTable: "Prods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prod_Logs_Tokens_FK_Tid",
                        column: x => x.FK_Tid,
                        principalTable: "Tokens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prod_Logs_Users_FK_Uid",
                        column: x => x.FK_Uid,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ShoppingCarts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_Tid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_Uid = table.Column<long>(type: "bigint", nullable: true),
                    FK_Pid = table.Column<long>(type: "bigint", nullable: false),
                    FK_S1id = table.Column<long>(type: "bigint", nullable: true),
                    FK_S2id = table.Column<long>(type: "bigint", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    Discont = table.Column<int>(type: "int", nullable: true),
                    Bonus = table.Column<int>(type: "int", nullable: true),
                    PriceType = table.Column<int>(type: "int", nullable: true),
                    IsAdditional = table.Column<bool>(type: "bit", nullable: false),
                    Ser_No = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ShoppingCarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingCarts_Prod_Specs_FK_S2id",
                        column: x => x.FK_S2id,
                        principalTable: "Prod_Specs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ShoppingCarts_Prods_FK_Pid",
                        column: x => x.FK_Pid,
                        principalTable: "Prods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShoppingCarts_Tokens_FK_Tid",
                        column: x => x.FK_Tid,
                        principalTable: "Tokens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShoppingCarts_Users_FK_Uid",
                        column: x => x.FK_Uid,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Stocks_FK_Pid",
                table: "Prod_Stocks",
                column: "FK_Pid");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Logs_FK_Pid",
                table: "Prod_Logs",
                column: "FK_Pid");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Logs_FK_Tid",
                table: "Prod_Logs",
                column: "FK_Tid");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Logs_FK_Uid",
                table: "Prod_Logs",
                column: "FK_Uid");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_FK_Pid",
                table: "ShoppingCarts",
                column: "FK_Pid");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_FK_S2id",
                table: "ShoppingCarts",
                column: "FK_S2id");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_FK_Tid",
                table: "ShoppingCarts",
                column: "FK_Tid");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_FK_Uid",
                table: "ShoppingCarts",
                column: "FK_Uid");

            migrationBuilder.AddForeignKey(
                name: "FK_LogisticsType_Payments_Logisticstypes_FK_Lid",
                table: "LogisticsType_Payments",
                column: "FK_Lid",
                principalTable: "Logisticstypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LogisticsType_Payments_PaymentTypes_FK_Pid",
                table: "LogisticsType_Payments",
                column: "FK_Pid",
                principalTable: "PaymentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prod_Specs_Prod_Spec_Types_FK_Tid",
                table: "Prod_Specs",
                column: "FK_Tid",
                principalTable: "Prod_Spec_Types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prod_Stocks_Prod_Specs_FK_S2id",
                table: "Prod_Stocks",
                column: "FK_S2id",
                principalTable: "Prod_Specs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Prod_Stocks_Prods_FK_Pid",
                table: "Prod_Stocks",
                column: "FK_Pid",
                principalTable: "Prods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ThirdPartyKeypairs_ThirdParties_FK_TPid",
                table: "ThirdPartyKeypairs",
                column: "FK_TPid",
                principalTable: "ThirdParties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogisticsType_Payments_Logisticstypes_FK_Lid",
                table: "LogisticsType_Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_LogisticsType_Payments_PaymentTypes_FK_Pid",
                table: "LogisticsType_Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Prod_Specs_Prod_Spec_Types_FK_Tid",
                table: "Prod_Specs");

            migrationBuilder.DropForeignKey(
                name: "FK_Prod_Stocks_Prod_Specs_FK_S2id",
                table: "Prod_Stocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Prod_Stocks_Prods_FK_Pid",
                table: "Prod_Stocks");

            migrationBuilder.DropForeignKey(
                name: "FK_ThirdPartyKeypairs_ThirdParties_FK_TPid",
                table: "ThirdPartyKeypairs");

            migrationBuilder.DropTable(
                name: "LogisticsSettings");

            migrationBuilder.DropTable(
                name: "Prod_Logs");

            migrationBuilder.DropTable(
                name: "ShoppingCarts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ThirdPartyKeypairs",
                table: "ThirdPartyKeypairs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ThirdParties",
                table: "ThirdParties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prod_Stocks",
                table: "Prod_Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Prod_Stocks_FK_Pid",
                table: "Prod_Stocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prod_Specs",
                table: "Prod_Specs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prod_Spec_Types",
                table: "Prod_Spec_Types");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentTypes",
                table: "PaymentTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Logisticstypes",
                table: "Logisticstypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LogisticsType_Payments",
                table: "LogisticsType_Payments");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Prods");

            migrationBuilder.DropColumn(
                name: "Introduction",
                table: "Prods");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Prods");

            migrationBuilder.RenameTable(
                name: "ThirdPartyKeypairs",
                newName: "ThirdPartyKeypair");

            migrationBuilder.RenameTable(
                name: "ThirdParties",
                newName: "ThirdParty");

            migrationBuilder.RenameTable(
                name: "Prod_Stocks",
                newName: "Prod_Stock");

            migrationBuilder.RenameTable(
                name: "Prod_Specs",
                newName: "Prod_Spec");

            migrationBuilder.RenameTable(
                name: "Prod_Spec_Types",
                newName: "Prod_Spec_Type");

            migrationBuilder.RenameTable(
                name: "PaymentTypes",
                newName: "PaymentType");

            migrationBuilder.RenameTable(
                name: "Logisticstypes",
                newName: "Logisticstype");

            migrationBuilder.RenameTable(
                name: "LogisticsType_Payments",
                newName: "LogisticsType_PaymentType");

            migrationBuilder.RenameIndex(
                name: "IX_ThirdPartyKeypairs_FK_TPid",
                table: "ThirdPartyKeypair",
                newName: "IX_ThirdPartyKeypair_FK_TPid");

            migrationBuilder.RenameIndex(
                name: "IX_Prod_Stocks_FK_S2id",
                table: "Prod_Stock",
                newName: "IX_Prod_Stock_FK_S2id");

            migrationBuilder.RenameIndex(
                name: "IX_Prod_Specs_FK_Tid",
                table: "Prod_Spec",
                newName: "IX_Prod_Spec_FK_Tid");

            migrationBuilder.RenameIndex(
                name: "IX_LogisticsType_Payments_FK_Pid",
                table: "LogisticsType_PaymentType",
                newName: "IX_LogisticsType_PaymentType_FK_Pid");

            migrationBuilder.RenameIndex(
                name: "IX_LogisticsType_Payments_FK_Lid",
                table: "LogisticsType_PaymentType",
                newName: "IX_LogisticsType_PaymentType_FK_Lid");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Prods",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3000)",
                oldMaxLength: 3000);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ThirdPartyKeypair",
                table: "ThirdPartyKeypair",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ThirdParty",
                table: "ThirdParty",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prod_Stock",
                table: "Prod_Stock",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prod_Spec",
                table: "Prod_Spec",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prod_Spec_Type",
                table: "Prod_Spec_Type",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentType",
                table: "PaymentType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Logisticstype",
                table: "Logisticstype",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LogisticsType_PaymentType",
                table: "LogisticsType_PaymentType",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LogisticsType_PaymentType_Logisticstype_FK_Lid",
                table: "LogisticsType_PaymentType",
                column: "FK_Lid",
                principalTable: "Logisticstype",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LogisticsType_PaymentType_PaymentType_FK_Pid",
                table: "LogisticsType_PaymentType",
                column: "FK_Pid",
                principalTable: "PaymentType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prod_Spec_Prod_Spec_Type_FK_Tid",
                table: "Prod_Spec",
                column: "FK_Tid",
                principalTable: "Prod_Spec_Type",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prod_Stock_Prod_Spec_FK_S2id",
                table: "Prod_Stock",
                column: "FK_S2id",
                principalTable: "Prod_Spec",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ThirdPartyKeypair_ThirdParty_FK_TPid",
                table: "ThirdPartyKeypair",
                column: "FK_TPid",
                principalTable: "ThirdParty",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

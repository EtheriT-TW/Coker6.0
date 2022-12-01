using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Add_Talbe_LogisticsProd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ser_no",
                table: "Prods",
                newName: "Ser_No");

            migrationBuilder.RenameColumn(
                name: "disp_opt",
                table: "Prods",
                newName: "Disp_Opt");

            migrationBuilder.CreateTable(
                name: "Logisticstype",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ecpaycode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_Logisticstype", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentType",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Head_column = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false),
                    Disp_Opt = table.Column<bool>(type: "bit", nullable: false),
                    Ser_No = table.Column<int>(type: "int", nullable: false),
                    ThirdID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ThirdKey = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_PaymentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prod_Spec_Type",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_Prod_Spec_Type", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThirdParty",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShopID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Account = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Code2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TaxID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Expire_Day = table.Column<int>(type: "int", nullable: true),
                    AuditUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PaymentUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TokenUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RefundUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MaxPay = table.Column<int>(type: "int", nullable: true),
                    Auto_Deposit = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ser_no = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ThirdParty", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsType_PaymentType",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_Lid = table.Column<long>(type: "bigint", maxLength: 50, nullable: false),
                    FK_Pid = table.Column<long>(type: "bigint", maxLength: 50, nullable: false),
                    Amountlimit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_LogisticsType_PaymentType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogisticsType_PaymentType_Logisticstype_FK_Lid",
                        column: x => x.FK_Lid,
                        principalTable: "Logisticstype",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LogisticsType_PaymentType_PaymentType_FK_Pid",
                        column: x => x.FK_Pid,
                        principalTable: "PaymentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prod_Spec",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_Tid = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_Prod_Spec", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prod_Spec_Prod_Spec_Type_FK_Tid",
                        column: x => x.FK_Tid,
                        principalTable: "Prod_Spec_Type",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThirdPartyKeypair",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_TPid = table.Column<long>(type: "bigint", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_ThirdPartyKeypair", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThirdPartyKeypair_ThirdParty_FK_TPid",
                        column: x => x.FK_TPid,
                        principalTable: "ThirdParty",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prod_Stock",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_Pid = table.Column<long>(type: "bigint", nullable: false),
                    FK_S1id = table.Column<long>(type: "bigint", nullable: true),
                    FK_S2id = table.Column<long>(type: "bigint", nullable: true),
                    Stock = table.Column<int>(type: "int", nullable: true),
                    Safe_Qty = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_Prod_Stock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prod_Stock_Prod_Spec_FK_S2id",
                        column: x => x.FK_S2id,
                        principalTable: "Prod_Spec",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsType_PaymentType_FK_Lid",
                table: "LogisticsType_PaymentType",
                column: "FK_Lid");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsType_PaymentType_FK_Pid",
                table: "LogisticsType_PaymentType",
                column: "FK_Pid");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Spec_FK_Tid",
                table: "Prod_Spec",
                column: "FK_Tid");

            migrationBuilder.CreateIndex(
                name: "IX_Prod_Stock_FK_S2id",
                table: "Prod_Stock",
                column: "FK_S2id");

            migrationBuilder.CreateIndex(
                name: "IX_ThirdPartyKeypair_FK_TPid",
                table: "ThirdPartyKeypair",
                column: "FK_TPid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogisticsType_PaymentType");

            migrationBuilder.DropTable(
                name: "Prod_Stock");

            migrationBuilder.DropTable(
                name: "ThirdPartyKeypair");

            migrationBuilder.DropTable(
                name: "Logisticstype");

            migrationBuilder.DropTable(
                name: "PaymentType");

            migrationBuilder.DropTable(
                name: "Prod_Spec");

            migrationBuilder.DropTable(
                name: "ThirdParty");

            migrationBuilder.DropTable(
                name: "Prod_Spec_Type");

            migrationBuilder.RenameColumn(
                name: "Ser_No",
                table: "Prods",
                newName: "ser_no");

            migrationBuilder.RenameColumn(
                name: "Disp_Opt",
                table: "Prods",
                newName: "disp_opt");
        }
    }
}

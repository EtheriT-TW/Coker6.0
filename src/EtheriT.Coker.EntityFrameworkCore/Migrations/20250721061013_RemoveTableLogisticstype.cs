using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTableLogisticstype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogisticsType_Payments_Logisticstypes_FK_Lid",
                table: "LogisticsType_Payments");

            migrationBuilder.DropTable(
                name: "Logisticstypes");

            migrationBuilder.DropIndex(
                name: "IX_LogisticsType_Payments_FK_Lid",
                table: "LogisticsType_Payments");

            migrationBuilder.DropColumn(
                name: "Amountlimit",
                table: "LogisticsType_Payments");

            migrationBuilder.DropColumn(
                name: "FK_Lid",
                table: "LogisticsType_Payments");

            migrationBuilder.AddColumn<int>(
                name: "ShippingType",
                table: "LogisticsType_Payments",
                type: "int",
                maxLength: 50,
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingType",
                table: "LogisticsType_Payments");

            migrationBuilder.AddColumn<string>(
                name: "Amountlimit",
                table: "LogisticsType_Payments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FK_Lid",
                table: "LogisticsType_Payments",
                type: "bigint",
                maxLength: 50,
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Logisticstypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ecpaycode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logisticstypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsType_Payments_FK_Lid",
                table: "LogisticsType_Payments",
                column: "FK_Lid");

            migrationBuilder.AddForeignKey(
                name: "FK_LogisticsType_Payments_Logisticstypes_FK_Lid",
                table: "LogisticsType_Payments",
                column: "FK_Lid",
                principalTable: "Logisticstypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

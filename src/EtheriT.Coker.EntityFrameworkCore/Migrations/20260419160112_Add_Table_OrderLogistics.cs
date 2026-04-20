using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_Table_OrderLogistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Order_Logistics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_OhId = table.Column<long>(type: "bigint", nullable: false),
                    LogisticsType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogisticsSubType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantTradeNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantTradeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AllPayLogisticsID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BookingNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSStoreID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSStoreName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSTelephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSOutSide = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSPaymentNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CVSValidationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GoodsWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Temperature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Specification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogisticsStatusCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateStatusDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_Logistics", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Order_Logistics");
        }
    }
}

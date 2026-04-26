using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_OrderHeader_RemoveLogisticsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllPayLogisticsID",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "BookingNote",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "CVSAddress",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "CVSOutSide",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "CVSPaymentNo",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "CVSStoreID",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "CVSStoreName",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "CVSTelephone",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "CVSValidationNo",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "LogisticsStatusCode",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "LogisticsUpdateStatusDate",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "MerchantTradeNo",
                table: "Order_Headers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AllPayLogisticsID",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BookingNote",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVSAddress",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVSOutSide",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVSPaymentNo",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVSStoreID",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVSStoreName",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVSTelephone",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVSValidationNo",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogisticsStatusCode",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LogisticsUpdateStatusDate",
                table: "Order_Headers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MerchantTradeNo",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

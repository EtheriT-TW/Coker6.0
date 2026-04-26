using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_OrderLogistics_AddColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MerchantTradeNo",
                table: "Order_Logistics",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverAddress",
                table: "Order_Logistics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverCellPhone",
                table: "Order_Logistics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverEmail",
                table: "Order_Logistics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverName",
                table: "Order_Logistics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverPhone",
                table: "Order_Logistics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverZipCode",
                table: "Order_Logistics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderAddress",
                table: "Order_Logistics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderCellPhone",
                table: "Order_Logistics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                table: "Order_Logistics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderPhone",
                table: "Order_Logistics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderZipCode",
                table: "Order_Logistics",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiverAddress",
                table: "Order_Logistics");

            migrationBuilder.DropColumn(
                name: "ReceiverCellPhone",
                table: "Order_Logistics");

            migrationBuilder.DropColumn(
                name: "ReceiverEmail",
                table: "Order_Logistics");

            migrationBuilder.DropColumn(
                name: "ReceiverName",
                table: "Order_Logistics");

            migrationBuilder.DropColumn(
                name: "ReceiverPhone",
                table: "Order_Logistics");

            migrationBuilder.DropColumn(
                name: "ReceiverZipCode",
                table: "Order_Logistics");

            migrationBuilder.DropColumn(
                name: "SenderAddress",
                table: "Order_Logistics");

            migrationBuilder.DropColumn(
                name: "SenderCellPhone",
                table: "Order_Logistics");

            migrationBuilder.DropColumn(
                name: "SenderName",
                table: "Order_Logistics");

            migrationBuilder.DropColumn(
                name: "SenderPhone",
                table: "Order_Logistics");

            migrationBuilder.DropColumn(
                name: "SenderZipCode",
                table: "Order_Logistics");

            migrationBuilder.AlterColumn<string>(
                name: "MerchantTradeNo",
                table: "Order_Logistics",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}

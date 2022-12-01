using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_ProdOrder_HeaderOrder_Details : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Discount",
                table: "Prods",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "disp_opt",
                table: "Prods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ser_no",
                table: "Prods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Invoice",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceAddress",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrdererAddress",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Remark",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UniformId",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Bonus",
                table: "Order_Details",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Subtotal",
                table: "Order_Details",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Prods");

            migrationBuilder.DropColumn(
                name: "disp_opt",
                table: "Prods");

            migrationBuilder.DropColumn(
                name: "ser_no",
                table: "Prods");

            migrationBuilder.DropColumn(
                name: "Invoice",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "InvoiceAddress",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "OrdererAddress",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "Remark",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "UniformId",
                table: "Order_Headers");

            migrationBuilder.DropColumn(
                name: "Bonus",
                table: "Order_Details");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Order_Details");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Table_Orde_Header : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Invoice",
                table: "Order_Headers",
                newName: "InvoiceTitle");

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceAddress",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceRecipient",
                table: "Order_Headers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceRecipient",
                table: "Order_Headers");

            migrationBuilder.RenameColumn(
                name: "InvoiceTitle",
                table: "Order_Headers",
                newName: "Invoice");

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceAddress",
                table: "Order_Headers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}

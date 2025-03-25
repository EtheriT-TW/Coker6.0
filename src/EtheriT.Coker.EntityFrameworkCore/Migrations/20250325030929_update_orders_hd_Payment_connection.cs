using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class update_orders_hd_Payment_connection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Payment",
                table: "Order_Headers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 2L,
                column: "memo",
                value: "請選擇需要翻譯的語系（請洽詢客服加購功能）");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Headers_Payment",
                table: "Order_Headers",
                column: "Payment");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Headers_PaymentTypes_Payment",
                table: "Order_Headers",
                column: "Payment",
                principalTable: "PaymentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Headers_PaymentTypes_Payment",
                table: "Order_Headers");

            migrationBuilder.DropIndex(
                name: "IX_Order_Headers_Payment",
                table: "Order_Headers");

            migrationBuilder.AlterColumn<int>(
                name: "Payment",
                table: "Order_Headers",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 2L,
                column: "memo",
                value: "請選擇需要翻譯的語系");
        }
    }
}

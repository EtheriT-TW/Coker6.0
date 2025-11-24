using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Alter_Order_head_AddConnect_shopping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Shipping",
                table: "Order_Headers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Headers_Shipping",
                table: "Order_Headers",
                column: "Shipping");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Headers_LogisticsSettings_Shipping",
                table: "Order_Headers",
                column: "Shipping",
                principalTable: "LogisticsSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Headers_LogisticsSettings_Shipping",
                table: "Order_Headers");

            migrationBuilder.DropIndex(
                name: "IX_Order_Headers_Shipping",
                table: "Order_Headers");

            migrationBuilder.AlterColumn<int>(
                name: "Shipping",
                table: "Order_Headers",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}

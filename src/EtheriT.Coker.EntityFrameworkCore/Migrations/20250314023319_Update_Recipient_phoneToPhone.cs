using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Update_Recipient_phoneToPhone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Telephone",
                table: "Recipients",
                newName: "TelePhone");

            migrationBuilder.RenameColumn(
                name: "Cellphone",
                table: "Recipients",
                newName: "CellPhone");

            migrationBuilder.RenameColumn(
                name: "RecipientTelephone",
                table: "Order_Headers",
                newName: "RecipientTelePhone");

            migrationBuilder.RenameColumn(
                name: "OrdererTelephone",
                table: "Order_Headers",
                newName: "OrdererTelePhone");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TelePhone",
                table: "Recipients",
                newName: "Telephone");

            migrationBuilder.RenameColumn(
                name: "CellPhone",
                table: "Recipients",
                newName: "Cellphone");

            migrationBuilder.RenameColumn(
                name: "RecipientTelePhone",
                table: "Order_Headers",
                newName: "RecipientTelephone");

            migrationBuilder.RenameColumn(
                name: "OrdererTelePhone",
                table: "Order_Headers",
                newName: "OrdererTelephone");
        }
    }
}

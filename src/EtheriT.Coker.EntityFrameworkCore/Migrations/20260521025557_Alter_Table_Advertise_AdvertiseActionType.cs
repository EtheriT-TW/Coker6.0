using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Alter_Table_Advertise_AdvertiseActionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActionType",
                table: "Advertise",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Css",
                table: "Advertise",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Html",
                table: "Advertise",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaveCss",
                table: "Advertise",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaveHtml",
                table: "Advertise",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "Advertise");

            migrationBuilder.DropColumn(
                name: "Css",
                table: "Advertise");

            migrationBuilder.DropColumn(
                name: "Html",
                table: "Advertise");

            migrationBuilder.DropColumn(
                name: "SaveCss",
                table: "Advertise");

            migrationBuilder.DropColumn(
                name: "SaveHtml",
                table: "Advertise");
        }
    }
}

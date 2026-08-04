using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AlterRecipientsCVSSColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CVSAddress",
                table: "Recipients",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVSOutSide",
                table: "Recipients",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVSStoreID",
                table: "Recipients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVSStoreName",
                table: "Recipients",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVSTelephone",
                table: "Recipients",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LogisticsType",
                table: "Recipients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Recipients",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CVSAddress",
                table: "Recipients");

            migrationBuilder.DropColumn(
                name: "CVSOutSide",
                table: "Recipients");

            migrationBuilder.DropColumn(
                name: "CVSStoreID",
                table: "Recipients");

            migrationBuilder.DropColumn(
                name: "CVSStoreName",
                table: "Recipients");

            migrationBuilder.DropColumn(
                name: "CVSTelephone",
                table: "Recipients");

            migrationBuilder.DropColumn(
                name: "LogisticsType",
                table: "Recipients");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Recipients");
        }
    }
}

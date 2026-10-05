using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddFrontAccountTraceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrowserInfo",
                table: "Account_Logs",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientIpAddress",
                table: "Account_Logs",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "Account_Logs",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentStatus",
                table: "Account_Logs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetailsJson",
                table: "Account_Logs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventName",
                table: "Account_Logs",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "Account_Logs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeyFingerprint",
                table: "Account_Logs",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewEmail",
                table: "Account_Logs",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldEmail",
                table: "Account_Logs",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousStatus",
                table: "Account_Logs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientEmail",
                table: "Account_Logs",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Success",
                table: "Account_Logs",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationMethod",
                table: "Account_Logs",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrowserInfo",
                table: "Account_Logs");

            migrationBuilder.DropColumn(
                name: "ClientIpAddress",
                table: "Account_Logs");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "Account_Logs");

            migrationBuilder.DropColumn(
                name: "CurrentStatus",
                table: "Account_Logs");

            migrationBuilder.DropColumn(
                name: "DetailsJson",
                table: "Account_Logs");

            migrationBuilder.DropColumn(
                name: "EventName",
                table: "Account_Logs");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "Account_Logs");

            migrationBuilder.DropColumn(
                name: "KeyFingerprint",
                table: "Account_Logs");

            migrationBuilder.DropColumn(
                name: "NewEmail",
                table: "Account_Logs");

            migrationBuilder.DropColumn(
                name: "OldEmail",
                table: "Account_Logs");

            migrationBuilder.DropColumn(
                name: "PreviousStatus",
                table: "Account_Logs");

            migrationBuilder.DropColumn(
                name: "RecipientEmail",
                table: "Account_Logs");

            migrationBuilder.DropColumn(
                name: "Success",
                table: "Account_Logs");

            migrationBuilder.DropColumn(
                name: "VerificationMethod",
                table: "Account_Logs");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Alter_Table_BonusLogDetail_RemoverOthersColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bonusLogDetails_BonusLog_FK_BonusLogs",
                table: "bonusLogDetails");

            migrationBuilder.RenameColumn(
                name: "FK_BonusLogs",
                table: "bonusLogDetails",
                newName: "FK_BonusLogsId");

            migrationBuilder.RenameIndex(
                name: "IX_bonusLogDetails_FK_BonusLogs",
                table: "bonusLogDetails",
                newName: "IX_bonusLogDetails_FK_BonusLogsId");

            migrationBuilder.AddForeignKey(
                name: "FK_bonusLogDetails_BonusLog_FK_BonusLogsId",
                table: "bonusLogDetails",
                column: "FK_BonusLogsId",
                principalTable: "BonusLog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropForeignKey(
                name: "FK_bonusLogDetails_BonusLog_BonusLogId",
                table: "bonusLogDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_bonusLogDetails_Bonus_BonusId",
                table: "bonusLogDetails");

            migrationBuilder.DropIndex(
                name: "IX_bonusLogDetails_BonusId",
                table: "bonusLogDetails");

            migrationBuilder.DropIndex(
                name: "IX_bonusLogDetails_BonusLogId",
                table: "bonusLogDetails");

            migrationBuilder.DropColumn(
                name: "BonusId",
                table: "bonusLogDetails");

            migrationBuilder.DropColumn(
                name: "BonusLogId",
                table: "bonusLogDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bonusLogDetails_BonusLog_FK_BonusLogsId",
                table: "bonusLogDetails");

            migrationBuilder.RenameColumn(
                name: "FK_BonusLogsId",
                table: "bonusLogDetails",
                newName: "FK_BonusLogs");

            migrationBuilder.RenameIndex(
                name: "IX_bonusLogDetails_FK_BonusLogsId",
                table: "bonusLogDetails",
                newName: "IX_bonusLogDetails_FK_BonusLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_bonusLogDetails_BonusLog_FK_BonusLogs",
                table: "bonusLogDetails",
                column: "FK_BonusLogs",
                principalTable: "BonusLog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

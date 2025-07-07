using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Alert_Table_BonusLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
            migrationBuilder.AddColumn<long>(
                name: "BonusId",
                table: "bonusLogDetails",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "BonusLogId",
                table: "bonusLogDetails",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_bonusLogDetails_BonusId",
                table: "bonusLogDetails",
                column: "BonusId");

            migrationBuilder.CreateIndex(
                name: "IX_bonusLogDetails_BonusLogId",
                table: "bonusLogDetails",
                column: "BonusLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_bonusLogDetails_BonusLog_BonusLogId",
                table: "bonusLogDetails",
                column: "BonusLogId",
                principalTable: "BonusLog",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_bonusLogDetails_Bonus_BonusId",
                table: "bonusLogDetails",
                column: "BonusId",
                principalTable: "Bonus",
                principalColumn: "Id");
        }
    }
}

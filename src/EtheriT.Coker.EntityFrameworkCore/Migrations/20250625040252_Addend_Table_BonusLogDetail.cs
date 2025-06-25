using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Addend_Table_BonusLogDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bonusLogDetails",
                columns: table => new
                {
                    FK_BonusId = table.Column<long>(type: "bigint", nullable: false),
                    FK_BonusLogs = table.Column<long>(type: "bigint", nullable: false),
                    UsedAmount = table.Column<long>(type: "bigint", nullable: false),
                    BonusId = table.Column<long>(type: "bigint", nullable: true),
                    BonusLogId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bonusLogDetails", x => new { x.FK_BonusId, x.FK_BonusLogs });
                    table.ForeignKey(
                        name: "FK_bonusLogDetails_BonusLog_BonusLogId",
                        column: x => x.BonusLogId,
                        principalTable: "BonusLog",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_bonusLogDetails_BonusLog_FK_BonusLogs",
                        column: x => x.FK_BonusLogs,
                        principalTable: "BonusLog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bonusLogDetails_Bonus_BonusId",
                        column: x => x.BonusId,
                        principalTable: "Bonus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_bonusLogDetails_Bonus_FK_BonusId",
                        column: x => x.FK_BonusId,
                        principalTable: "Bonus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bonusLogDetails_BonusId",
                table: "bonusLogDetails",
                column: "BonusId");

            migrationBuilder.CreateIndex(
                name: "IX_bonusLogDetails_BonusLogId",
                table: "bonusLogDetails",
                column: "BonusLogId");

            migrationBuilder.CreateIndex(
                name: "IX_bonusLogDetails_FK_BonusLogs",
                table: "bonusLogDetails",
                column: "FK_BonusLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bonusLogDetails");
        }
    }
}

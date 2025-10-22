using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Added_Table_MappingLogisticsSettingAndProd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FreigntStatusType",
                table: "LogisticsSettings",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "MappingLogisticsSettingAndProd",
                columns: table => new
                {
                    FK_LogisticsSettingId = table.Column<long>(type: "bigint", nullable: false),
                    FK_ProdId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappingLogisticsSettingAndProd", x => new { x.FK_LogisticsSettingId, x.FK_ProdId });
                    table.ForeignKey(
                        name: "FK_MappingLogisticsSettingAndProd_LogisticsSettings_FK_LogisticsSettingId",
                        column: x => x.FK_LogisticsSettingId,
                        principalTable: "LogisticsSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MappingLogisticsSettingAndProd_Prods_FK_ProdId",
                        column: x => x.FK_ProdId,
                        principalTable: "Prods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MappingLogisticsSettingAndProd_FK_ProdId",
                table: "MappingLogisticsSettingAndProd",
                column: "FK_ProdId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MappingLogisticsSettingAndProd");

            migrationBuilder.DropColumn(
                name: "FreigntStatusType",
                table: "LogisticsSettings");
        }
    }
}

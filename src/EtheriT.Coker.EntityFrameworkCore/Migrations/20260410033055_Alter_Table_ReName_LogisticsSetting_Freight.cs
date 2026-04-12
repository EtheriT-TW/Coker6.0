using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Alter_Table_ReName_LogisticsSetting_Freight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FreigntType",
                table: "LogisticsSettings",
                newName: "FreightType");

            migrationBuilder.RenameColumn(
                name: "FreigntStatusType",
                table: "LogisticsSettings",
                newName: "FreightStatusType");

            migrationBuilder.RenameColumn(
                name: "FreigntAmt2",
                table: "LogisticsSettings",
                newName: "FreightAmt2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FreightType",
                table: "LogisticsSettings",
                newName: "FreigntType");

            migrationBuilder.RenameColumn(
                name: "FreightStatusType",
                table: "LogisticsSettings",
                newName: "FreigntStatusType");

            migrationBuilder.RenameColumn(
                name: "FreightAmt2",
                table: "LogisticsSettings",
                newName: "FreigntAmt2");
        }
    }
}

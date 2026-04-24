using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Alter_Table_LogisticsSetting_DiscountFreightType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscountFreightType",
                table: "LogisticsSettings",
                type: "int",
                nullable: true,
                defaultValue: 1);

            migrationBuilder.Sql(@"
                UPDATE LogisticsSettings
                SET DiscountFreightType = 1
                WHERE DiscountFreightType IS NULL
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountFreightType",
                table: "LogisticsSettings");
        }
    }
}

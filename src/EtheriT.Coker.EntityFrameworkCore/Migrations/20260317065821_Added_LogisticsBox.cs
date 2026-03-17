using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Added_LogisticsBox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PackingPoint",
                table: "Prod_Stocks",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "LogisticsBoxs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CapacityPoint = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Sort = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogisticsBoxs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogisticsBoxs_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogisticsBoxFees",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_LogisticsSettingId = table.Column<long>(type: "bigint", nullable: false),
                    FK_LogisticsBoxId = table.Column<long>(type: "bigint", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogisticsBoxFees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogisticsBoxFees_LogisticsBoxs_FK_LogisticsBoxId",
                        column: x => x.FK_LogisticsBoxId,
                        principalTable: "LogisticsBoxs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LogisticsBoxFees_LogisticsSettings_FK_LogisticsSettingId",
                        column: x => x.FK_LogisticsSettingId,
                        principalTable: "LogisticsSettings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsBoxFees_FK_LogisticsBoxId_FK_LogisticsSettingId",
                table: "LogisticsBoxFees",
                columns: new[] { "FK_LogisticsBoxId", "FK_LogisticsSettingId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsBoxFees_FK_LogisticsSettingId",
                table: "LogisticsBoxFees",
                column: "FK_LogisticsSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsBoxs_FK_WebsiteId_CapacityPoint",
                table: "LogisticsBoxs",
                columns: new[] { "FK_WebsiteId", "CapacityPoint" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogisticsBoxFees");

            migrationBuilder.DropTable(
                name: "LogisticsBoxs");

            migrationBuilder.DropColumn(
                name: "PackingPoint",
                table: "Prod_Stocks");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddTableMarketingRewardItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequiredQuantityPerQualification",
                table: "MarketingScopeItems",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "MaxSelectionQuantityPerOrder",
                table: "MarketingRewards",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SelectionQuantityPerQualification",
                table: "MarketingRewards",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "MarketingRewardItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_MarketingRewardId = table.Column<long>(type: "bigint", nullable: false),
                    FK_ProdStockId = table.Column<long>(type: "bigint", nullable: false),
                    OfferPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxQuantityPerOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_MarketingRewardItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketingRewardItems_MarketingRewards_FK_MarketingRewardId",
                        column: x => x.FK_MarketingRewardId,
                        principalTable: "MarketingRewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarketingRewardItems_Prod_Stocks_FK_ProdStockId",
                        column: x => x.FK_ProdStockId,
                        principalTable: "Prod_Stocks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketingRewardItems_FK_MarketingRewardId_Enabled_SortOrder",
                table: "MarketingRewardItems",
                columns: new[] { "FK_MarketingRewardId", "Enabled", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketingRewardItems_FK_MarketingRewardId_FK_ProdStockId",
                table: "MarketingRewardItems",
                columns: new[] { "FK_MarketingRewardId", "FK_ProdStockId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingRewardItems_FK_ProdStockId",
                table: "MarketingRewardItems",
                column: "FK_ProdStockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketingRewardItems");

            migrationBuilder.DropColumn(
                name: "RequiredQuantityPerQualification",
                table: "MarketingScopeItems");

            migrationBuilder.DropColumn(
                name: "MaxSelectionQuantityPerOrder",
                table: "MarketingRewards");

            migrationBuilder.DropColumn(
                name: "SelectionQuantityPerQualification",
                table: "MarketingRewards");
        }
    }
}

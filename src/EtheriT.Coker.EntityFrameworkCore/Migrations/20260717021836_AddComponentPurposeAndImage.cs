using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddComponentPurposeAndImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComponentPurposes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SerNo = table.Column<int>(type: "int", nullable: false),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ComponentPurposes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HtmlContentPurposes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_HtmlContentId = table.Column<long>(type: "bigint", nullable: false),
                    FK_ComponentPurposeId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_HtmlContentPurposes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HtmlContentPurposes_ComponentPurposes_FK_ComponentPurposeId",
                        column: x => x.FK_ComponentPurposeId,
                        principalTable: "ComponentPurposes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HtmlContentPurposes_Html_Contents_FK_HtmlContentId",
                        column: x => x.FK_HtmlContentId,
                        principalTable: "Html_Contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ComponentPurposes",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "Name", "SerNo", "Visible" },
                values: new object[] { 1L, "product-import-directory", new DateTime(2026, 7, 17, 0, 0, 0, 0, DateTimeKind.Local), 2L, null, null, null, null, "商品匯入目錄", 10, true });

            migrationBuilder.CreateIndex(
                name: "IX_ComponentPurposes_Code",
                table: "ComponentPurposes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HtmlContentPurposes_FK_ComponentPurposeId",
                table: "HtmlContentPurposes",
                column: "FK_ComponentPurposeId");

            migrationBuilder.CreateIndex(
                name: "IX_HtmlContentPurposes_FK_HtmlContentId_FK_ComponentPurposeId",
                table: "HtmlContentPurposes",
                columns: new[] { "FK_HtmlContentId", "FK_ComponentPurposeId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HtmlContentPurposes");

            migrationBuilder.DropTable(
                name: "ComponentPurposes");
        }
    }
}

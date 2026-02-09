using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Added_table_DirectoryFacetRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CalendarType",
                table: "Directory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "FK_DefaultLayout",
                table: "Directory",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FacetType",
                table: "Directory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DirectoryFacetRanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_DirectoryId = table.Column<long>(type: "bigint", nullable: false),
                    Sort = table.Column<int>(type: "int", nullable: false),
                    Start = table.Column<int>(type: "int", nullable: false),
                    End = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_DirectoryFacetRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DirectoryFacetRanges_Directory_FK_DirectoryId",
                        column: x => x.FK_DirectoryId,
                        principalTable: "Directory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Directory_FacetType",
                table: "Directory",
                column: "FacetType");

            migrationBuilder.CreateIndex(
                name: "IX_Directory_FK_DefaultLayout",
                table: "Directory",
                column: "FK_DefaultLayout");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryFacetRanges_FK_DirectoryId",
                table: "DirectoryFacetRanges",
                column: "FK_DirectoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Directory_Html_Contents_FK_DefaultLayout",
                table: "Directory",
                column: "FK_DefaultLayout",
                principalTable: "Html_Contents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Directory_Html_Contents_FK_DefaultLayout",
                table: "Directory");

            migrationBuilder.DropTable(
                name: "DirectoryFacetRanges");

            migrationBuilder.DropIndex(
                name: "IX_Directory_FacetType",
                table: "Directory");

            migrationBuilder.DropIndex(
                name: "IX_Directory_FK_DefaultLayout",
                table: "Directory");

            migrationBuilder.DropColumn(
                name: "CalendarType",
                table: "Directory");

            migrationBuilder.DropColumn(
                name: "FK_DefaultLayout",
                table: "Directory");

            migrationBuilder.DropColumn(
                name: "FacetType",
                table: "Directory");
        }
    }
}

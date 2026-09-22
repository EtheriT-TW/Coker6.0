using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class FileCleanupAndRecycleBin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileCleanupCandidates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_FileUploadId = table.Column<long>(type: "bigint", nullable: false),
                    FirstDetectedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastConfirmedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("PK_FileCleanupCandidates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileRecycleBinItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_FileUploadId = table.Column<long>(type: "bigint", nullable: false),
                    RecycledTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OriginalPath = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("PK_FileRecycleBinItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileCleanupCandidates_FK_WebsiteId_FK_FileUploadId",
                table: "FileCleanupCandidates",
                columns: new[] { "FK_WebsiteId", "FK_FileUploadId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_FileCleanupCandidates_FK_WebsiteId_LastConfirmedTime",
                table: "FileCleanupCandidates",
                columns: new[] { "FK_WebsiteId", "LastConfirmedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_FileRecycleBinItems_FK_WebsiteId_FK_FileUploadId",
                table: "FileRecycleBinItems",
                columns: new[] { "FK_WebsiteId", "FK_FileUploadId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecycleBinItems_FK_WebsiteId_RecycledTime",
                table: "FileRecycleBinItems",
                columns: new[] { "FK_WebsiteId", "RecycledTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileCleanupCandidates");

            migrationBuilder.DropTable(
                name: "FileRecycleBinItems");
        }
    }
}

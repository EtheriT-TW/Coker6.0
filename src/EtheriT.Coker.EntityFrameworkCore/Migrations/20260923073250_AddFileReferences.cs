using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddFileReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileReferences",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_FileUploadId = table.Column<long>(type: "bigint", nullable: true),
                    SourceType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SourceId = table.Column<long>(type: "bigint", nullable: false),
                    SourceState = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SourceField = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NormalizedPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OccurrenceCount = table.Column<int>(type: "int", nullable: false),
                    PhysicalFileExists = table.Column<bool>(type: "bit", nullable: false),
                    LastConfirmedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_FileReferences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileReferences_FK_WebsiteId_FK_FileUploadId",
                table: "FileReferences",
                columns: new[] { "FK_WebsiteId", "FK_FileUploadId" });

            migrationBuilder.CreateIndex(
                name: "IX_FileReferences_FK_WebsiteId_LastConfirmedTime",
                table: "FileReferences",
                columns: new[] { "FK_WebsiteId", "LastConfirmedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_FileReferences_FK_WebsiteId_SourceType_SourceId_SourceState",
                table: "FileReferences",
                columns: new[] { "FK_WebsiteId", "SourceType", "SourceId", "SourceState" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileReferences");
        }
    }
}

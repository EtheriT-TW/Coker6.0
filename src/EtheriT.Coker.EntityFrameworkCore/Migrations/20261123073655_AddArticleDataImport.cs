using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleDataImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArticleTemplateKey",
                table: "Directory",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FieldDataJson",
                table: "Article",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ArticleImportBatches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_DirectoryId = table.Column<long>(type: "bigint", nullable: false),
                    FK_BackgroundTaskId = table.Column<long>(type: "bigint", nullable: false),
                    FK_ConfirmTaskId = table.Column<long>(type: "bigint", nullable: true),
                    TemplateKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SourceFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ColumnsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_ArticleImportBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticleImportRows",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_ArticleImportBatchId = table.Column<long>(type: "bigint", nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SourceJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FieldDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DownloadedFilesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FK_ArticleId = table.Column<long>(type: "bigint", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_ArticleImportRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleImportRows_ArticleImportBatches_FK_ArticleImportBatchId",
                        column: x => x.FK_ArticleImportBatchId,
                        principalTable: "ArticleImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleImportBatches_FK_BackgroundTaskId",
                table: "ArticleImportBatches",
                column: "FK_BackgroundTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleImportBatches_FK_WebsiteId_FK_DirectoryId",
                table: "ArticleImportBatches",
                columns: new[] { "FK_WebsiteId", "FK_DirectoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleImportRows_FK_ArticleImportBatchId_RowNumber",
                table: "ArticleImportRows",
                columns: new[] { "FK_ArticleImportBatchId", "RowNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleImportRows");

            migrationBuilder.DropTable(
                name: "ArticleImportBatches");

            migrationBuilder.DropColumn(
                name: "ArticleTemplateKey",
                table: "Directory");

            migrationBuilder.DropColumn(
                name: "FieldDataJson",
                table: "Article");
        }
    }
}

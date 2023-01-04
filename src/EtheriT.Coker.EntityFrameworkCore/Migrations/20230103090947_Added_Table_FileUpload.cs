using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Table_FileUpload : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileUploads",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    GuidKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DownloadFileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Size = table.Column<int>(type: "int", nullable: false),
                    FileGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileUploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileUploads_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileUploads_FK_WebsiteId",
                table: "FileUploads",
                column: "FK_WebsiteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileUploads");
        }
    }
}

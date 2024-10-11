using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Table_Account_Log : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Account_Logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorTimes = table.Column<int>(type: "int", nullable: false),
                    LockTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    mappingFrontUserAndWebsiteId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Account_Logs_MappingFrontUserAndWebsite_mappingFrontUserAndWebsiteId",
                        column: x => x.mappingFrontUserAndWebsiteId,
                        principalTable: "MappingFrontUserAndWebsite",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Account_Logs_Websites_WebsiteId",
                        column: x => x.WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_Logs_mappingFrontUserAndWebsiteId",
                table: "Account_Logs",
                column: "mappingFrontUserAndWebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Account_Logs_WebsiteId",
                table: "Account_Logs",
                column: "WebsiteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Account_Logs");
        }
    }
}

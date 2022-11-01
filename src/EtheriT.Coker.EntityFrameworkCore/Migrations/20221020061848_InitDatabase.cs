using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class InitDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Account = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CellPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Websites",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Locale = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Websites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MappingUserAndWebsites",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappingUserAndWebsites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MappingUserAndWebsites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MappingUserAndWebsites_Websites_WebsiteId",
                        column: x => x.WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Account", "CellPhone", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "Email", "LastModificationTime", "LastModifierUserId", "Name", "Password" },
                values: new object[,]
                {
                    { 1L, "EtheriT", "0906801568", new DateTime(2022, 10, 20, 14, 18, 47, 872, DateTimeKind.Local).AddTicks(6452), 0L, null, null, "service@ether.com.tw", null, null, "易碩網際科技科技股份有限公司", "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" },
                    { 2L, "lcb", "0920497649", new DateTime(2022, 10, 20, 14, 18, 47, 872, DateTimeKind.Local).AddTicks(6460), 0L, null, null, "lienmienchou@evergreen.com.tw", null, null, "隆昌窯業", "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" }
                });

            migrationBuilder.InsertData(
                table: "Websites",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DefaultUrl", "DeleterUserId", "DeletionTime", "Description", "Icon", "Keywords", "LastModificationTime", "LastModifierUserId", "Locale", "Title", "Type" },
                values: new object[,]
                {
                    { 1L, new DateTime(2022, 10, 20, 14, 18, 47, 872, DateTimeKind.Local).AddTicks(6550), 0L, null, null, null, null, null, null, null, null, "zh-tw", "Coker雲端開店大師", "website" },
                    { 2L, new DateTime(2022, 10, 20, 14, 18, 47, 872, DateTimeKind.Local).AddTicks(6552), 0L, null, null, null, null, null, null, null, null, "zh-tw", "｜Derek｜德瑞克．隆昌窯業", "website" }
                });

            migrationBuilder.InsertData(
                table: "MappingUserAndWebsites",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "UserId", "WebsiteId" },
                values: new object[] { 1L, new DateTime(2022, 10, 20, 14, 18, 47, 872, DateTimeKind.Local).AddTicks(6563), 0L, null, null, null, null, 1L, 1L });

            migrationBuilder.InsertData(
                table: "MappingUserAndWebsites",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "UserId", "WebsiteId" },
                values: new object[] { 2L, new DateTime(2022, 10, 20, 14, 18, 47, 872, DateTimeKind.Local).AddTicks(6564), 0L, null, null, null, null, 2L, 2L });

            migrationBuilder.CreateIndex(
                name: "IX_MappingUserAndWebsites_UserId",
                table: "MappingUserAndWebsites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingUserAndWebsites_WebsiteId",
                table: "MappingUserAndWebsites",
                column: "WebsiteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MappingUserAndWebsites");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Websites");
        }
    }
}

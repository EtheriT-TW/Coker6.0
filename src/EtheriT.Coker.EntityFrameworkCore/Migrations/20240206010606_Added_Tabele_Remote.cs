using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Tabele_Remote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Remotes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    FK_UserId = table.Column<long>(type: "bigint", nullable: true),
                    FK_WebmenuId = table.Column<long>(type: "bigint", nullable: false),
                    FK_ArticleId = table.Column<long>(type: "bigint", nullable: true),
                    FK_ProdId = table.Column<long>(type: "bigint", nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClientIpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    BrowserInfo = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Remotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Remotes_Article_FK_ArticleId",
                        column: x => x.FK_ArticleId,
                        principalTable: "Article",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Remotes_Prods_FK_ProdId",
                        column: x => x.FK_ProdId,
                        principalTable: "Prods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Remotes_Users_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Remotes_WebMenus_FK_WebmenuId",
                        column: x => x.FK_WebmenuId,
                        principalTable: "WebMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_FK_ArticleId",
                table: "Remotes",
                column: "FK_ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_FK_ProdId",
                table: "Remotes",
                column: "FK_ProdId");

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_FK_UserId",
                table: "Remotes",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Remotes_FK_WebmenuId",
                table: "Remotes",
                column: "FK_WebmenuId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Remotes");
        }
    }
}

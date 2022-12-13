using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Add_Table_HtmlContent_AND_LogisticsSetting_Add_FKWebid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FK_WebsiteId",
                table: "LogisticsSettings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Html_Contents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    Menu_id = table.Column<long>(type: "bigint", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Img = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ser_no = table.Column<int>(type: "int", nullable: false),
                    Disp_opt = table.Column<bool>(type: "bit", nullable: false),
                    ObjectType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Target = table.Column<bool>(type: "bit", nullable: false),
                    Link = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    permanent = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Html_Contents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Html_Contents_Websites_FK_WebsiteId",
                        column: x => x.FK_WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogisticsSettings_FK_WebsiteId",
                table: "LogisticsSettings",
                column: "FK_WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Html_Contents_FK_WebsiteId",
                table: "Html_Contents",
                column: "FK_WebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_LogisticsSettings_Websites_FK_WebsiteId",
                table: "LogisticsSettings",
                column: "FK_WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogisticsSettings_Websites_FK_WebsiteId",
                table: "LogisticsSettings");

            migrationBuilder.DropTable(
                name: "Html_Contents");

            migrationBuilder.DropIndex(
                name: "IX_LogisticsSettings_FK_WebsiteId",
                table: "LogisticsSettings");

            migrationBuilder.DropColumn(
                name: "FK_WebsiteId",
                table: "LogisticsSettings");
        }
    }
}

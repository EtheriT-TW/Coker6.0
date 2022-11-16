using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Table_WebMenus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebMenus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    SerNO = table.Column<int>(type: "int", nullable: false),
                    Popular = table.Column<int>(type: "int", nullable: false),
                    PopularVisible = table.Column<bool>(type: "bit", nullable: false),
                    ImgId = table.Column<long>(type: "bigint", nullable: true),
                    OverImgId = table.Column<long>(type: "bigint", nullable: true),
                    LinkUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LanBar = table.Column<bool>(type: "bit", nullable: false),
                    FK_TopNodeId = table.Column<long>(type: "bigint", nullable: true),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebMenus_WebMenus_FK_TopNodeId",
                        column: x => x.FK_TopNodeId,
                        principalTable: "WebMenus",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "MappingUserAndWebsites",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1457));

            migrationBuilder.UpdateData(
                table: "MappingUserAndWebsites",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1458));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreationTime", "Password" },
                values: new object[] { new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1328), "AQAAAAEAACcQAAAAEE3X/SrNcUs6zaH9K+51XEMp8G2z3r9d/5SYuLJpKy3TlYNX7DdHF6PDW8NxWk7CWg==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "CreationTime", "Password" },
                values: new object[] { new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1338), "AQAAAAEAACcQAAAAEE3X/SrNcUs6zaH9K+51XEMp8G2z3r9d/5SYuLJpKy3TlYNX7DdHF6PDW8NxWk7CWg==" });

            migrationBuilder.UpdateData(
                table: "Websites",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1441));

            migrationBuilder.UpdateData(
                table: "Websites",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1443));

            migrationBuilder.CreateIndex(
                name: "IX_WebMenus_FK_TopNodeId",
                table: "WebMenus",
                column: "FK_TopNodeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebMenus");

            migrationBuilder.UpdateData(
                table: "MappingUserAndWebsites",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 18, 11, 21, 208, DateTimeKind.Local).AddTicks(1688));

            migrationBuilder.UpdateData(
                table: "MappingUserAndWebsites",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 18, 11, 21, 208, DateTimeKind.Local).AddTicks(1689));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreationTime", "Password" },
                values: new object[] { new DateTime(2022, 11, 1, 18, 11, 21, 208, DateTimeKind.Local).AddTicks(1576), "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "CreationTime", "Password" },
                values: new object[] { new DateTime(2022, 11, 1, 18, 11, 21, 208, DateTimeKind.Local).AddTicks(1586), "AM4OLBpptxBYmM79lGOX9egzZk3vIQU3d/gFCJzaBjAPXzYIK3tQ2N7X4fcrHtElTw==" });

            migrationBuilder.UpdateData(
                table: "Websites",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 18, 11, 21, 208, DateTimeKind.Local).AddTicks(1676));

            migrationBuilder.UpdateData(
                table: "Websites",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 18, 11, 21, 208, DateTimeKind.Local).AddTicks(1677));
        }
    }
}

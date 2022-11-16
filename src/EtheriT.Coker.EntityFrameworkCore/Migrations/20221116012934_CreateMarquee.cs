using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class CreateMarquee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Marquees",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WebsiteId = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    disp_opt = table.Column<bool>(type: "bit", nullable: false),
                    ser_no = table.Column<int>(type: "int", nullable: false),
                    link = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    target = table.Column<bool>(type: "bit", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marquees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Marquees_Websites_WebsiteId",
                        column: x => x.WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "MappingUserAndWebsites",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 16, 9, 29, 33, 983, DateTimeKind.Local).AddTicks(9814));

            migrationBuilder.UpdateData(
                table: "MappingUserAndWebsites",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 16, 9, 29, 33, 983, DateTimeKind.Local).AddTicks(9815));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 16, 9, 29, 33, 983, DateTimeKind.Local).AddTicks(9626));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 16, 9, 29, 33, 983, DateTimeKind.Local).AddTicks(9640));

            migrationBuilder.UpdateData(
                table: "Websites",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 16, 9, 29, 33, 983, DateTimeKind.Local).AddTicks(9756));

            migrationBuilder.UpdateData(
                table: "Websites",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 16, 9, 29, 33, 983, DateTimeKind.Local).AddTicks(9798));

            migrationBuilder.CreateIndex(
                name: "IX_Marquees_WebsiteId",
                table: "Marquees",
                column: "WebsiteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Marquees");

            migrationBuilder.UpdateData(
                table: "MappingUserAndWebsites",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 17, 1, 23, 702, DateTimeKind.Local).AddTicks(8672));

            migrationBuilder.UpdateData(
                table: "MappingUserAndWebsites",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 17, 1, 23, 702, DateTimeKind.Local).AddTicks(8673));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 17, 1, 23, 702, DateTimeKind.Local).AddTicks(8560));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 17, 1, 23, 702, DateTimeKind.Local).AddTicks(8568));

            migrationBuilder.UpdateData(
                table: "Websites",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 17, 1, 23, 702, DateTimeKind.Local).AddTicks(8659));

            migrationBuilder.UpdateData(
                table: "Websites",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreationTime",
                value: new DateTime(2022, 11, 1, 17, 1, 23, 702, DateTimeKind.Local).AddTicks(8661));
        }
    }
}

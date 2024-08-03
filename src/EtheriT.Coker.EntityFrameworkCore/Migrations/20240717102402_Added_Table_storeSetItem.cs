using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    public partial class Added_Table_storeSetItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoreSetItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FK_StoreSetId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_StoreSetItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreSetItems_StoreSet_FK_StoreSetId",
                        column: x => x.FK_StoreSetId,
                        principalTable: "StoreSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "pattern", "type" },
                values: new object[] { "^G-\\w", 1 });

            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "IsDeleted", "LastModificationTime", "LastModifierUserId", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 2L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 1L, false, null, null, "S002", "google.translate", 50, "請選擇需要翻譯的語系", "Google自動翻譯", "(?=[a-z]{2}-?[A-Z]{0,2},?)+", 4 });

            migrationBuilder.InsertData(
                table: "StoreSetItems",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetId", "IsDeleted", "Key", "LastModificationTime", "LastModifierUserId", "Value" },
                values: new object[,]
                {
                    { 1L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, "zh-TW", null, null, "中文(繁體)" },
                    { 2L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, "zh-CN", null, null, "中文(簡體)" },
                    { 3L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, "en", null, null, "英文" },
                    { 4L, new DateTime(2024, 7, 17, 18, 4, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 2L, false, "ja", null, null, "日文" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoreSetItems_FK_StoreSetId",
                table: "StoreSetItems",
                column: "FK_StoreSetId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoreSetItems");

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "pattern", "type" },
                values: new object[] { "\\d6-\\d", 7 });
        }
    }
}

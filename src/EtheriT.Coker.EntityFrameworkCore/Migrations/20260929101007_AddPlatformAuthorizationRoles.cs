using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformAuthorizationRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlatformRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Sort = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_PlatformRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MappingUserAndPlatformRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    PlatformRoleId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_MappingUserAndPlatformRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MappingUserAndPlatformRoles_PlatformRoles_PlatformRoleId",
                        column: x => x.PlatformRoleId,
                        principalTable: "PlatformRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MappingUserAndPlatformRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "PlatformRoles",
                columns: new[] { "Id", "Code", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "Description", "IsEnabled", "LastModificationTime", "LastModifierUserId", "Name", "Sort" },
                values: new object[,]
                {
                    { 1L, "PlatformAdministrator", new DateTime(2026, 9, 29, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, "可管理 Platform 所有功能、角色與伺服器操作。", true, null, null, "Platform 總管理者", 10 },
                    { 2L, "PlatformDataManager", new DateTime(2026, 9, 29, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, "可管理客戶、網站與網域資料，不可操作伺服器或管理 Platform 角色。", true, null, null, "網站資料管理人員", 20 },
                    { 3L, "PlatformServerOperator", new DateTime(2026, 9, 29, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, "可檢視監控並執行經確認的主機、IIS、DNS 與 SSL 操作。", true, null, null, "伺服器操作管理員", 30 }
                });

            migrationBuilder.InsertData(
                table: "MappingUserAndPlatformRoles",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "LastModificationTime", "LastModifierUserId", "PlatformRoleId", "UserId" },
                values: new object[] { 1L, new DateTime(2026, 9, 29, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, null, null, 1L, 1L });

            migrationBuilder.CreateIndex(
                name: "IX_MappingUserAndPlatformRoles_PlatformRoleId",
                table: "MappingUserAndPlatformRoles",
                column: "PlatformRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_MappingUserAndPlatformRoles_UserId_PlatformRoleId",
                table: "MappingUserAndPlatformRoles",
                columns: new[] { "UserId", "PlatformRoleId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRoles_Code",
                table: "PlatformRoles",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MappingUserAndPlatformRoles");

            migrationBuilder.DropTable(
                name: "PlatformRoles");
        }
    }
}

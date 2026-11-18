using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddClaudeWorkspaceIdSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DefaultValue", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "LastModificationTime", "LastModifierUserId", "Level", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 40L, new DateTime(2026, 11, 18, 0, 0, 0, 0, DateTimeKind.Local), 1L, "", null, null, 9L, null, null, null, "A005", "AiClaudeWorkspaceId", 100, "選填；未綁定工作區的 Claude API Key 必須指定工作區 ID", "Claude 工作區 ID", "^wrkspc_[a-zA-Z0-9]+$", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 40L);
        }
    }
}

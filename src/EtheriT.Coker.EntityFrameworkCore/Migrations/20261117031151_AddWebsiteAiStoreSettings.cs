using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddWebsiteAiStoreSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StoreSetGroup",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "Description", "Image", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[] { 9L, new DateTime(2026, 11, 17, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, "設定本站 AI 輔助功能與服務供應商", "", null, null, "AI 設定" });

            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DefaultValue", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "LastModificationTime", "LastModifierUserId", "Level", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[,]
                {
                    { 36L, new DateTime(2026, 11, 17, 0, 0, 0, 0, DateTimeKind.Local), 1L, "false", null, null, 9L, null, null, null, "A001", "AiEnabled", 5, "預設關閉，由管理者開放後才能使用", "啟用 AI 輔助", "", 4 },
                    { 37L, new DateTime(2026, 11, 17, 0, 0, 0, 0, DateTimeKind.Local), 1L, "self", null, null, 9L, null, null, null, "A002", "AiProvider", 20, "未設定外部金鑰時使用系統預設 AI", "AI 供應商", "", 5 },
                    { 38L, new DateTime(2026, 11, 17, 0, 0, 0, 0, DateTimeKind.Local), 1L, "", null, null, 9L, null, null, null, "A003", "AiModel", 150, "外部 AI 使用的模型 ID", "模型名稱", "^[a-zA-Z0-9][a-zA-Z0-9._:-]*$", 1 },
                    { 39L, new DateTime(2026, 11, 17, 0, 0, 0, 0, DateTimeKind.Local), 1L, "", null, null, 9L, null, null, null, "A004", "AiApiKey", 4096, "由 AI 專用設定頁加密保存；一般查詢不回填金鑰", "API Key", "", 10 }
                });

            migrationBuilder.InsertData(
                table: "StoreSetItems",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetId", "IsDefault", "Key", "LastModificationTime", "LastModifierUserId", "Level", "Value" },
                values: new object[,]
                {
                    { 33L, new DateTime(2026, 11, 17, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, 37L, true, "self", null, null, null, "系統預設 AI" },
                    { 34L, new DateTime(2026, 11, 17, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, 37L, false, "openai", null, null, null, "OpenAI（ChatGPT）" },
                    { 35L, new DateTime(2026, 11, 17, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, 37L, false, "gemini", null, null, null, "Google Gemini" },
                    { 36L, new DateTime(2026, 11, 17, 0, 0, 0, 0, DateTimeKind.Local), 1L, null, null, 37L, false, "claude", null, null, null, "Anthropic Claude" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 38L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 39L);



            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 33L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 34L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 35L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 36L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 36L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 37L);

            migrationBuilder.DeleteData(
                table: "StoreSetGroup",
                keyColumn: "Id",
                keyValue: 9L);
        }
    }
}

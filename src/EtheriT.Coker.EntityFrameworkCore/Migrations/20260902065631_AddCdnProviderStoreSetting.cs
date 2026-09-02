using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddCdnProviderStoreSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StoreSetGroup",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "Description", "Image", "LastModificationTime", "LastModifierUserId", "Title" },
                values: new object[] { 8L, new DateTime(2026, 9, 2, 12, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, "設定網站目前使用的 CDN 服務商", "", null, null, "CDN 設定" });

            migrationBuilder.InsertData(
                table: "StoreSet",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DefaultValue", "DeleterUserId", "DeletionTime", "FK_StoreSetGroupId", "LastModificationTime", "LastModifierUserId", "Level", "jobID", "key", "maxlength", "memo", "name", "pattern", "type" },
                values: new object[] { 34L, new DateTime(2026, 9, 2, 12, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, "None", null, null, 8L, null, null, null, "S003", "cdnProvider", 30, "此設定只供系統辨識 CDN 來源，不會自動變更 DNS、啟用外部服務或調整伺服器防火牆。請在 CDN 設定完成後選擇對應服務商。", "CDN 服務商", "", 3 });

            migrationBuilder.InsertData(
                table: "StoreSetItems",
                columns: new[] { "Id", "CreationTime", "CreatorUserId", "DeleterUserId", "DeletionTime", "FK_StoreSetId", "IsDefault", "Key", "LastModificationTime", "LastModifierUserId", "Level", "Value" },
                values: new object[,]
                {
                    { 22L, new DateTime(2026, 9, 2, 12, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 34L, true, "None", null, null, null, "未使用 CDN" },
                    { 23L, new DateTime(2026, 9, 2, 12, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 34L, false, "Cloudflare", null, null, null, "Cloudflare" },
                    { 24L, new DateTime(2026, 9, 2, 12, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 34L, false, "CloudFront", null, null, null, "AWS CloudFront" },
                    { 25L, new DateTime(2026, 9, 2, 12, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 34L, false, "AzureFrontDoor", null, null, null, "Azure Front Door" },
                    { 26L, new DateTime(2026, 9, 2, 12, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 34L, false, "GoogleCloudCdn", null, null, null, "Google Cloud CDN" },
                    { 27L, new DateTime(2026, 9, 2, 12, 0, 0, 0, DateTimeKind.Local).AddTicks(1459), 1L, null, null, 34L, false, "Fastly", null, null, null, "Fastly" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 22L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 23L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 24L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 25L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 26L);

            migrationBuilder.DeleteData(
                table: "StoreSetItems",
                keyColumn: "Id",
                keyValue: 27L);

            migrationBuilder.DeleteData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 34L);

            migrationBuilder.DeleteData(
                table: "StoreSetGroup",
                keyColumn: "Id",
                keyValue: 8L);
        }
    }
}

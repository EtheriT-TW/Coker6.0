using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicalCertificateDraftContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SaveCss",
                table: "TechnicalCertificates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaveHtml",
                table: "TechnicalCertificates",
                type: "nvarchar(max)",
                nullable: true);

            // 發布內容使用 /upload/，儲存稿使用所屬網站的 /upload/{OrgName}/。
            // 同時支援已編碼的 HTML 屬性與 CSS url()，避免修改外部網址。
            static string DraftUploadPaths(string column)
            {
                var expression = $"COALESCE({column}, N'')";
                foreach (var prefix in new[] { "\"", "'", "&quot;", "&#34;", "&#39;", "&#x27;", "&apos;", "url(" })
                {
                    var sqlPrefix = prefix.Replace("'", "''");
                    // 只正規化根相對路徑，已帶本站 OrgName 的網址不重複加上站台目錄。
                    expression = $"REPLACE({expression}, N'{sqlPrefix}/upload/' + w.[OrgName] + N'/', N'{sqlPrefix}/upload/')";
                    expression = $"REPLACE({expression}, N'{sqlPrefix}/upload/', N'{sqlPrefix}/upload/' + w.[OrgName] + N'/')";
                }

                return $"CASE WHEN NULLIF(LTRIM(RTRIM(w.[OrgName])), N'') IS NULL THEN COALESCE({column}, N'') ELSE {expression} END";
            }

            migrationBuilder.Sql($@"
                UPDATE tc
                SET [SaveHtml] = CASE WHEN tc.[SaveHtml] IS NULL THEN {DraftUploadPaths("tc.[Html]")} ELSE tc.[SaveHtml] END,
                    [SaveCss] = CASE WHEN tc.[SaveCss] IS NULL THEN {DraftUploadPaths("tc.[Css]")} ELSE tc.[SaveCss] END
                FROM [TechnicalCertificates] AS tc
                LEFT JOIN [Websites] AS w ON w.[Id] = tc.[FK_WebsiteId]
                WHERE tc.[SaveHtml] IS NULL OR tc.[SaveCss] IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaveCss",
                table: "TechnicalCertificates");

            migrationBuilder.DropColumn(
                name: "SaveHtml",
                table: "TechnicalCertificates");
        }
    }
}

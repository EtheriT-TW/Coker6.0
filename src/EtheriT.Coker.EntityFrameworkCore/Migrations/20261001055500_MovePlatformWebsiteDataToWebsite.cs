using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class MovePlatformWebsiteDataToWebsite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ① 已綁定有效站台的網站資料：平台欄位複製到 Website。
            //    名稱／版本／語系／日期不複製，以 Website 為準（Platform 畫面原本就這樣顯示）。
            //    PlatformWebsites.FK_WebsiteId 有唯一索引（排除已刪除），一個站台最多對到一筆。
            //    可重跑：同樣的值寫兩次結果相同。
            migrationBuilder.Sql(@"
UPDATE w
SET w.ServiceStatus       = pw.Status,
    w.TerminatedDate      = pw.TerminatedDate,
    w.HostLocation        = pw.HostLocation,
    w.IsDomainPending     = pw.IsDomainPending,
    w.FK_PlatformDomainId = pw.FK_PlatformDomainId,
    w.PlatformRemark      = pw.Remark
FROM Websites w
INNER JOIN PlatformWebsites pw
        ON pw.FK_WebsiteId = w.Id
       AND pw.IsDeleted = 0
WHERE w.IsDeleted = 0;");

            // ② 後台還沒綁公司的站台，用網站資料上的客戶補建綁定（客戶已刪除的不補）。
            //    可重跑：NOT EXISTS 擋掉已經有綁定的站台。
            migrationBuilder.Sql(@"
INSERT INTO MappingCompanyAndWebsites (FK_CompanyId, FK_WebsiteId, CreatorUserId, CreationTime, IsDeleted)
SELECT pw.FK_CompanyId, pw.FK_WebsiteId, pw.CreatorUserId, GETDATE(), 0
FROM PlatformWebsites pw
INNER JOIN Websites w  ON w.Id = pw.FK_WebsiteId AND w.IsDeleted = 0
INNER JOIN Companies c ON c.Id = pw.FK_CompanyId AND c.IsDeleted = 0
WHERE pw.IsDeleted = 0
  AND NOT EXISTS (SELECT 1 FROM MappingCompanyAndWebsites m
                  WHERE m.FK_WebsiteId = pw.FK_WebsiteId AND m.IsDeleted = 0);");

            // ③ 未綁定、或綁到已刪除站台的網站資料不處理：Platform 從此不讀 PlatformWebsites，
            //    整張表原封不動留作備份，第四階段才刪除。
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 只把平台欄位清回預設值。PlatformWebsites 從未被修改，退回舊版 Platform 即可恢復原狀。
            // ② 補建的公司綁定不移除：內容與原網站資料的客戶相同，舊版 Platform 存檔時也會建立同樣的綁定。
            migrationBuilder.Sql(@"
UPDATE Websites
SET ServiceStatus = 0,
    TerminatedDate = NULL,
    HostLocation = NULL,
    IsDomainPending = 0,
    FK_PlatformDomainId = NULL,
    PlatformRemark = NULL;");
        }
    }
}
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class PlatformWebsite : FullAuditedEntity
    {
        // ── 客戶與網站資料 ──
        public long FK_CompanyId { get; set; }                          // 客戶（Companies）
        [StringLength(250)]
        public string Name { get; set; } = string.Empty;                // 網站名稱（必填）

        // ── 版本與期限 ──
        public WebsiteLevelEnum? Level { get; set; }                    // 網站版本
        [StringLength(200)]
        public string? HostLocation { get; set; }                       // 主機位置
        public DateTime? ServiceStartDate { get; set; }                 // 網站開通日期
        public DateTime? ServiceEndDate { get; set; }                   // 網站到期日期
        public PlatformWebsiteStatusEnum Status { get; set; }           // 正常／暫停／註銷
        public DateTime? TerminatedDate { get; set; }                   // 註銷日期

        // ── 網址 ──
        public bool IsDomainPending { get; set; }                       // 網域待申請
        [StringLength(500)]
        public string? Url { get; set; }                                // 網址（使用者輸入原文）
        public long? FK_PlatformDomainId { get; set; }                  // 後端比對出的網域；待申請或未填網址時為 null

        // ── 備註 ──
        [StringLength(2000)]
        public string? Remark { get; set; }

        // ── 預留：對應實際站台 ──
        public long? FK_WebsiteId { get; set; }                         // nullable，不建外鍵約束，預設 null

        public Company? Company { get; set; }
        public PlatformDomain? Domain { get; set; }
    }
}
using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    /// <summary>
    /// 客戶管理平台（Web.Platform）維護：站台「主要網址以外」的其他網址。
    /// 主要網址仍是 Website.DefaultUrl（金流、物流回呼、寄信都讀它），這裡不重複存。
    /// 前台與後台都不讀這張表。
    /// </summary>
    public class WebsiteAlternateUrl : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        [StringLength(255)]
        public string Url { get; set; } = string.Empty;     // 根網址，例：https://www.example.com.tw
        public long? FK_PlatformDomainId { get; set; }      // 依網址比對出的網域；網域管理中還沒建立時為 null
        public int Sort { get; set; }                       // 排序

        public Website? Website { get; set; }
        public PlatformDomain? PlatformDomain { get; set; }
    }
}
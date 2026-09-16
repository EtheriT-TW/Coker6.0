using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;
using EtheriT.Coker.Application.Shared.Dto.enumType;

namespace EtheriT.Coker.Core.Models
{
    public class PlatformCustomer : FullAuditedEntity
    {
        // ── 客戶資料 ──
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;   // 公司名稱（必填）
        [StringLength(20)]
        public string TaxId { get; set; } = string.Empty;  // 統一編號（必填）
        [StringLength(50)]
        public string? Phone { get; set; }                 // 公司電話
        [StringLength(150)]
        public string? Email { get; set; }                 // 公司Email
        [StringLength(250)]
        public string? Address { get; set; }               // 公司地址
        [StringLength(500)]
        public string? InvoiceInfo { get; set; }           // 發票資訊
        [StringLength(50)]
        public string? SalesOwner { get; set; }            // 負責業務
        public PlatformCustomerTypeEnum CustomerType { get; set; }          // 客戶屬性
        [StringLength(50)]
        public string? CustomerTypeOther { get; set; }     // 選「其他」時的自填

        // ── 主要聯絡人 ──
        [StringLength(100)]
        public string? PrimaryContactName { get; set; }
        [StringLength(100)]
        public string? PrimaryContactJobTitle { get; set; }
        [StringLength(50)]
        public string? PrimaryContactPhone { get; set; }
        [StringLength(150)]
        public string? PrimaryContactEmail { get; set; }

        public List<PlatformCustomerContact> SubContacts { get; set; } = [];
    }
}
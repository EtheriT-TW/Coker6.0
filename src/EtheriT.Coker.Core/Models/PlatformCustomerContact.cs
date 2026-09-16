using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class PlatformCustomerContact : FullAuditedEntity
    {
        public long FK_PlatformCustomerId { get; set; }

        [StringLength(100)]
        public string Name { get; set; } = string.Empty;   // 姓名（必填）
        [StringLength(100)]
        public string? JobTitle { get; set; }              // 職稱
        [StringLength(50)]
        public string? Phone { get; set; }                 // 連絡電話
        [StringLength(150)]
        public string? Email { get; set; }                 // Email
        public int Sort { get; set; }                      // 排序

        public PlatformCustomer? Customer { get; set; }
    }
}
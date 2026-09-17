using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class PlatformDomain : FullAuditedEntity
    {
        [StringLength(255)]
        public string DomainName { get; set; } = string.Empty;          // 網域（小寫主機名稱，不含 http:// 與路徑）
        [StringLength(200)]
        public string? Registrar { get; set; }                          // 網域公司
        public DateTime? StartDate { get; set; }                        // 網域起始日期
        public DateTime? EndDate { get; set; }                          // 網域到期日期
        [StringLength(1000)]
        public string? PasswordCipher { get; set; }                     // 網域密碼（密文）
        [StringLength(2000)]
        public string? Remark { get; set; }
    }
}
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    /// <summary>企業信箱，綁客戶（Companies），一個客戶最多一筆未刪除的。</summary>
    public class WebsiteEnterpriseEmail : FullAuditedEntity
    {
        public long FK_CompanyId { get; set; }
        public virtual Company? Company { get; set; }
        public EnterpriseEmailTypeEnum AccountType { get; set; }    //帳號類型存Enum
        public int TotalAccounts { get; set; }                      //帳號總數
        [StringLength(200)]
        public string? OtherType { get; set; }                      //存帳號類型=其他時內容
        public DateTime ExpirationDate { get; set; }                //到期日
        [StringLength(2000)]
        public string? Remark { get; set; }                         //備註
    }
}

using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
	public class ThirdParty : FullAuditedEntity
    {
        [StringLength(50)] public string ShopID { get; set; }
        [StringLength(50)] public string Account { get; set; }
        [StringLength(100)] public string? Code1 { get; set; }
        [StringLength(100)] public string? Code2 { get; set; }
        [StringLength(50)] public string? Password { get; set; }
        [StringLength(10)] public string? TaxID { get; set; }
        public int? Expire_Day { get; set; }
        [StringLength(255)] public string? AuditUrl { get; set; }
        [StringLength(255)] public string? PaymentUrl { get; set; }
        [StringLength(255)] public string? TokenUrl { get; set; }
        [StringLength(255)] public string? RefundUrl { get; set; }
        public int? MaxPay { get; set; }
        public bool Auto_Deposit { get; set; }
        public int Status { get; set; }
        [StringLength(50)] public string Title { get; set; }
        public int ser_no { get; set; }
        public List<ThirdPartyKeypair> ThirdPartyKeypair { get; set; }
    }
}

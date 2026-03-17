using EtheriT.Coker.Application.Shared.Dto.enumType.ThirdParty;
using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
	public class ThirdParty : FullAuditedEntity
    {
        [StringLength(255)] public string? AuditUrl { get; set; }
        [StringLength(255)] public string? PaymentUrl { get; set; }
        [StringLength(255)] public string? TokenUrl { get; set; }
        [StringLength(255)] public string? RefundUrl { get; set; }
        public int? MaxPay { get; set; }
        [StringLength(50)] public string Title { get; set; }
        public int ser_no { get; set; }
        public ThirdPartyServiceTypeEnum ServiceType { get; set; }
        public List<ThirdPartyKeypair> ThirdPartyKeypair { get; set; }
        public List<PaymentType> paymentTypes { get; set; }
    }
}

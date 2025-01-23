using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class PaymentType : FullAuditedEntity
    {
        [StringLength(50)] public string? Title { get; set; }
        [StringLength(50)] public string? Code { get; set; }
        public bool Used { get; set; }
        public int SerNo { get; set; }
        public int MinAmount { get; set; } = 1;
        public int? MaxAmount { get; set; }
        public string Icons { get; set; }
        public long FK_ThirdPartyId { get; set; }
        public ThirdParty? ThirdParty { get; set; }
        public List<LogisticsType_PaymentType>? LogisticsType_Payments { get; set; }
        public List<PaymentTypesValue>? paymentTypesValues { get; set; }
    }
}

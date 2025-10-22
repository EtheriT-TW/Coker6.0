using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class LogisticsPaymentRestriction : FullAuditedEntity
    {
        [StringLength(50)] public ShippingTypeEnum ShippingType { get; set; }
        [StringLength(50)] public long FK_Pid { get; set; }
        public PaymentType? PaymentType { get; set; }
    }
}

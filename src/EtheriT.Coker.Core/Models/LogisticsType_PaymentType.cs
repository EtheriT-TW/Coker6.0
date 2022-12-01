using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class LogisticsType_PaymentType : FullAuditedEntity
    {
        [StringLength(50)] public long FK_Lid { get; set; }
        [StringLength(50)] public long FK_Pid { get; set; }
        [StringLength(50)] public string? Amountlimit { get; set; }
        public Logisticstype? Logisticstype { get; set; }
        public PaymentType? PaymentType { get; set; }
    }
}

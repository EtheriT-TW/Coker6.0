using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
	public class Logisticstype : FullAuditedEntity
    {
        [StringLength(50)] public string Title { get; set; }
        [StringLength(50)] public string? Ecpaycode { get; set; }
        public List<LogisticsType_PaymentType> LogisticsType_Payments { get; set; }
    }
}

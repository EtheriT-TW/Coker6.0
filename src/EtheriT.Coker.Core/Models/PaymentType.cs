using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class PaymentType : FullAuditedEntity
    {
        [StringLength(50)] public string? Title { get; set; }
        [StringLength(50)] public string? Code { get; set; }
        [StringLength(50)] public string? Head_column { get; set; }
        public bool Used { get; set; }
        public bool Disp_Opt { get; set; }
        public int Ser_No { get; set; }
        [StringLength(50)] public string? ThirdID { get; set; }
        public int? ThirdKey { get; set; }
        public List<LogisticsType_PaymentType> LogisticsType_Payments { get; set; }
    }
}

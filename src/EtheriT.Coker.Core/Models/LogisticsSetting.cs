using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class LogisticsSetting : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        [StringLength(50)] public string Title { get; set; }
        public int PreserveType { get; set; }
        public int LogisticsType { get; set; }
        public int FreigntType { get; set; }
        public int? Freight { get; set; }
        public int? Low_Con { get; set; }
        public int? Dis_Freight { get; set; }
        public bool Set_Default { get; set; }
        public int? FreigntAmt2 { get; set; }
        public Website? Website { get; set; }
    }
}

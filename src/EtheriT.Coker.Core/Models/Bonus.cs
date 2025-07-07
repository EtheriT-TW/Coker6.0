using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    public class Bonus : FullAuditedEntity
    {
        public int Amount { get; set; }
        public Guid UUID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Note { get; set; }
        public int Balance { get; set; }
        public List<BonusLogDetail> BonusLogDetails { get; set; } = new List<BonusLogDetail>();
    }
}

using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    public class Bonus : FullAuditedEntity
    {
        public double Amount { get; set; }
        public Guid UUID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Note { get; set; }
        public double Balance { get; set; }
    }
}

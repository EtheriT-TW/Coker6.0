
using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    public class ShoppingCart : FullAuditedEntity
    {
        public string Token { get; set; }
        public long? FK_Cid { get; set; }
        public long FK_Pid { get; set; }
        public long? FK_S1id { get; set; }
        public long? FK_S2id { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public int? Discont { get; set; }
        public int? Bonus { get; set; }
        public int? PriceType { get; set; }
        public bool IsAdditional { get; set; }
        public int Ser_No { get; set; }
    }
}

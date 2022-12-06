using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    public class Prod_Stock : FullAuditedEntity
    {
        public long FK_Pid { get; set; }
        public long? FK_S1id { get; set; }
        public long? FK_S2id { get; set; }
        public int? Stock { get; set; }
        public int? Safe_Qty { get; set; }
        public int Ser_No { get; set; }
        public Prod? Prod { get; set; }
        public Prod_Spec? Prod_Spec { get; set; }
        public List<ShoppingCart> ShoppingCarts { get; set; }
        public List<Order_Details> Order_Details { get; set; }

    }
}

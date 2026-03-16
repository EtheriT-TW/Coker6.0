using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    public class Prod_Stock : FullAuditedEntity
    {
        public long FK_Pid { get; set; }
        public long? FK_S1id { get; set; }
        public long? FK_S2id { get; set; }
        public decimal Price { get; set; }
        public int? Stock { get; set; }
        public int? Alert_Qty { get; set; }
        public int? Min_Qty { get; set; }
        public int Ser_No { get; set; }
        public int PackingPoint { get; set; }
        public bool IsTimePrice { get; set; }
        public string? SubItemNo { get; set; }
        public Prod? Prod { get; set; }
        public List<ShoppingCart> ShoppingCarts { get; set; }
        public List<Prod_Price> Prod_Prices { get; set; }

    }
}

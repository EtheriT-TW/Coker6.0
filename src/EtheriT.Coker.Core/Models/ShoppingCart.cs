
using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Web.Core.Models;

namespace EtheriT.Coker.Core.Models
{
    public class ShoppingCart : FullAuditedEntity
    {
        public Guid FK_Tid { get; set; }
        public Guid UUID { get; set; }
        public long? FK_Uid { get; set; }
        public long FK_PSid { get; set; }
        public long? FK_PriceId { get; set; }
        public long? FK_S1id { get; set; }
        public long? FK_S2id { get; set; }
        public int OldQuantity { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public int? Discont { get; set; }
        public int? Bonus { get; set; }
        public int? PriceType { get; set; }
        public bool IsAdditional { get; set; }
        public int Ser_No { get; set; }
        // 是否已結帳
        public bool IsOrder { get; set; }
        // 結帳後才存入
        public string? ProdName { get; set; }
        public ICollection<Token> Tokens { get; set; }
        public Prod_Stock Prod_Stock { get; set; }
        public Prod_Price? Prod_Price { get; set; }
        public List<Order_Details> Order_Details { get; set; }
    }
}

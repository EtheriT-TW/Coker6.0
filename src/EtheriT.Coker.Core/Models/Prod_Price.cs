
using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
	public class Prod_Price : FullAuditedEntity
    {
		public long FK_PSId { get; set; }
        public long FK_RId { get; set; }
        public double? Price { get; set; }
        public double? Bonus { get; set; }
        public Prod_Stock? Prod_Stock { get; set; }
        public Role? Role { get; set; }
        public List<ShoppingCart> ShoppingCarts { get; set; }
    }
}

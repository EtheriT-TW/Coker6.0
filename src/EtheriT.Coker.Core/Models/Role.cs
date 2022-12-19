using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
	public class Role : FullAuditedEntity
    {
        public string Name { get; set; }
        public List<Prod_Price> Prod_Prices { get; set; }

    }
}

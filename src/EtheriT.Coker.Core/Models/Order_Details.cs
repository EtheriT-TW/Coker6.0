using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
	public class Order_Details : FullAuditedEntity
    {
        public long FK_OId { get; set; }
        public long FK_SCId { get; set; }
        public Order_Header? Order_Header { get; set; }
        public ShoppingCart? ShoppingCart { get; set; }
    }
}

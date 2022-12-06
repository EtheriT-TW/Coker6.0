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
        public long FK_PSId { get; set; }
        public int Amount { get; set; }
        public double Subtotal { get; set; }
        public int? Bonus { get; set; }
        public Order_Header? Order_Header { get; set; }
        public Prod_Stock? Prod_Stock { get; set; }
    }
}

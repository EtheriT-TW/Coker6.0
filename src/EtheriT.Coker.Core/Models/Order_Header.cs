using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EtheriT.Coker.Core.Models
{
    public class Order_Header : FullAuditedEntity
    {
        public string Orderer { get; set; }
        public int OrdererSex { get; set; }
        public string OrdererEmail { get; set; }
        public string? OrdererTelephone { get; set; }
        public string OrdererCellPhone { get; set; }
        public string OrdererAddress { get; set; }
        public string Recipient { get; set; }
        public string RecipientEmail { get; set; }
        public string? RecipientTelephone { get; set; }
        public string RecipientCellPhone { get; set; }
        public string RecipientAddress { get; set; }
        public string? Remark { get; set; }
        public string? Invoice { get; set; }
        public string? UniformId { get; set; }
        public string? InvoiceAddress { get; set; }
        public string State { get; set; }
        public int Total { get; set; }
        public int? Discount { get; set; }
        public int? Bonus { get; set; }
        public long? CouponId { get; set; }
        public int Freight { get; set; }
        public int? Service_Charge { get; set; }
        public List<Order_Details> Order_Details { get; set; }

    }
}

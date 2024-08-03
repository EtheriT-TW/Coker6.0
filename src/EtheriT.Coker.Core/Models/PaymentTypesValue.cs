using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class PaymentTypesValue: FullAuditedEntity
    {
        public bool Used { get; set; }
        public long FK_WebsiteId {  get; set; }
        public long FK_PaymentTypesId {  get; set; }
        public PaymentType paymentType { get; set; }
        public Website website { get; set; }
    }
}

using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class Prod : FullAuditedEntity
    {
        [StringLength(150)]
        public string Title { get; set; }
        public bool disp_opt { get; set; }
        public int ser_no { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public double? Discount { get; set; }
        public List<Order_Details> Order_Details { get; set; }

    }
}

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
        public bool Disp_Opt { get; set; }
        public int Ser_No { get; set; }
        [StringLength(3000)]
        public string Introduction { get; set; }
        [StringLength(3000)]
        public string Description { get; set; }
        public double Price { get; set; }
        public double? Discount { get; set; }
        public virtual DateTime? StartTime { get; set; }
        public virtual DateTime? EndTime { get; set; }
        public List<Order_Details> Order_Details { get; set; }
        public List<Prod_Stock> Prod_Stocks { get; set; }
        public List<ShoppingCart> ShoppingCarts { get; set; }
        public List<Prod_Log> Prod_Logs { get; set; }

    }
}

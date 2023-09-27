using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class CustSearch : FullAuditedEntity
    {
        [StringLength(100)]
        public string? Title { get; set; }
        public int SerNo { get; set; }
        public bool SearchAllProd { get; set; }
        public bool SearchAllArticle { get; set; }
        public bool SearchAllMenu { get; set; }
        public bool Visible { get; set; }
        [StringLength(100)]
        public string Placeholder { get; set; }
        public long FK_WebsiteId { get; set; }
        public Website Website { get; set; }
    }
}

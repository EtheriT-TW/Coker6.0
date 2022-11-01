using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class Prod: FullAuditedEntity
    {
        [StringLength(150)]
        public string Title { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }

    }
}

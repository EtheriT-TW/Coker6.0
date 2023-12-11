using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class Company : FullAuditedEntity
    {
        public string Name { get; set; }
        [StringLength(10)]
        public string TaxID { get; set; }
        [StringLength(100)]
        public string Contact { get; set; }
        [StringLength(150)]
        public string Email { get; set; }
        [StringLength(150)]
        public string Address { get; set; }
        public List<MappingCompanyAndWebsites> Websites { get; set; }
    }
}

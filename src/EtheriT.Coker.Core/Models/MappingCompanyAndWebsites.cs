using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Web.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class MappingCompanyAndWebsites : FullAuditedEntity
    {
        public long FK_CompanyId { get; set; }
        public long FK_WebsiteId { get; set; }
        public Company? Company { get; set; }
        public Website? Website { get; set; }
    }
}

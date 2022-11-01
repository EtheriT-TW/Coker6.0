using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Web.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class MappingUserAndWebsite: FullAuditedEntity
    {
        public long UserId { get; set; }
        public long WebsiteId { get; set; }
        public User? User { get; set; }
        public Website? Website { get; set; }
    }
}

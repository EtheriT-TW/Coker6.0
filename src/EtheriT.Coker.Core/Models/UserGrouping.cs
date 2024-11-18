using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class UserGrouping: FullAuditedEntity
    {
        public string Title {  get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long FK_WebsiteId {  get; set; }
        public bool Enable {  get; set; }
    }
}

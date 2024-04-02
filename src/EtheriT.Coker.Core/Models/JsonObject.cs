using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class JsonObject : FullAuditedEntity
    {
        public int Type {  get; set; }
        public string Json { get; set; }
        public long? FK_AId { get; set; }
        public long FK_WebsiteId {  get; set; }
        public Website FK_Website { get; set; }
    }
}

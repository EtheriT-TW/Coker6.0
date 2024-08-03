using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class ThirdPartyKeypairValue : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public long FK_ThirdPartyKeypairId { get; set; }
        [StringLength(300)] public string Value { get; set; } = string.Empty;
        public Website Website { get; set; }
        public ThirdPartyKeypair ThirdPartyKeypair { get; set; }
    }
}

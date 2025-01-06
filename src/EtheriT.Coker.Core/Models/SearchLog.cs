using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class SearchLog
    {
        public long Id { get; set; }
        public Guid UUID { get; set; }
        public long FK_WebsiteId { get; set; }
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;
        public long FK_CustSearchId { get; set; }
        [MaxLength(255)]
        public string ClientIpAddress { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public Website Website { get; set; }
    }
}

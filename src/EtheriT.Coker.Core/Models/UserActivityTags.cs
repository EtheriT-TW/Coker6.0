using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class UserActivityTags
    {
        public Guid Id { get; set; }
        public long FK_RemoteId { get; set; }
        public long FK_TId { get; set; }
        public float Weight { get; set; }
        public DateTime CreateTime { get; set; }
        public Remote Remote { get; set; }
    }
}

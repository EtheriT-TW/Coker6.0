using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class Account_Log
    {
        public virtual long Id { get; set; }
        public Guid UUID { get; set; }
        public long WebsiteId { get; set; }
        public int Status { get; set; } 
        public int ErrorTimes { get; set; }
        public DateTime? LockTime { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public virtual long CreatorUserId { get; set; }
        public virtual DateTime CreationTime { get; set; } = DateTime.Now;
        public Website? Website { get; set; }
    }
}

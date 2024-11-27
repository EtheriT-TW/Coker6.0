using EtheriT.Coker.Web.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class UserTagStatistic
    {
        public long Id { get; set; }
        public Guid UUID { get; set; }
        public long FK_TagId { get; set; }
        public int TotalTimes { get; set; }
        public DateTime LastActivityTime { get; set; }
        public float Weight { get; set; }
        public Tag Tag { get; set; }
    }
}

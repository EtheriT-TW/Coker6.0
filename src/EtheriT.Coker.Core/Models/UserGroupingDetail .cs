using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class UserGroupingDetail
    {
        public Guid UUID { get; set; }
        public long FK_GropingId {  get; set; }
        public UserGrouping userGrouping { get; set; }
    }
}

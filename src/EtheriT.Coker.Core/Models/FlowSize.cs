using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class FlowSize
    {
		public long Id {  get; set; }
		public long FK_WebsiteId { get; set; }
		public long RequestSize { get; set; }
		public long ResponseSize { get; set; }
		public long Total { get; set; }
		public DateTime actionTime { get; set; }
		public Website Website { get; set; }
	}
}

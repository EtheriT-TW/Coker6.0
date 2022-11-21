using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Marquee
{
	public class MarqueeUpdateDto
    {
        public long Id { get; set; }
        public long WebsiteId { get; set; }
        public string title { get; set; }
        public bool disp_opt { get; set; }
        public int ser_no { get; set; }
        public string link { get; set; }
        public bool target { get; set; }
        public virtual DateTime StartTime { get; set; }
        public virtual DateTime EndTime { get; set; }
    }
}

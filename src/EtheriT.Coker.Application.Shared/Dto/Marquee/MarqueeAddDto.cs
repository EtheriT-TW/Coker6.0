
using EtheriT.Coker.Application.Dto;

namespace EtheriT.Coker.Application.Shared.Dto.Marquee
{
	public class MarqueeAddDto
    {
        public long WebsiteId { get; set; }
        public string title { get; set; }
        public bool disp_opt { get; set; }
        public int ser_no { get; set; }
        public string link { get; set; }
        public bool target { get; set; }
        public virtual DateTime? StartTime { get; set; }
        public virtual DateTime? EndTime { get; set; }
        public bool permanent { get; set; }
    }
}

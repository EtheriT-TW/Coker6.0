using EtheriT.Coker.Application.Shared.Dto.Newsletter;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Advertise
{
    public class AdvertiseGetDataDto
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string Link { get; set; }
        public bool Target { get; set; }
        public bool Visible { get; set; }
        public int SerNO { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool permanent { get; set; }
        public long FK_WebsiteId { get; set; }
        public List<TagGetSelectedDto> TagDatas { get; set; }
    }
}

using EtheriT.Coker.Application.Shared.Dto.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Advertise
{
    public class AdvertiseDto
    {
        public long? Id { get; set; }
        public string Title { get; set; }
        public string? Link { get; set; }
        public bool Target { get; set; } = false;
        public bool Visible { get; set; }
        public int SerNO { get; set; } = 500;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool permanent { get; set; } = false;
        public List<TagSelectedDto> TagSelected { get; set; }
    }
}
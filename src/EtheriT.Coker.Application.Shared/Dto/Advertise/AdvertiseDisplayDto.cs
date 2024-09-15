using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Newsletter;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Advertise
{
    public class AdvertiseDisplayDto
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string Link { get; set; }
        public bool Target { get; set; }
        public int Exposure { get; set; }
        public int Clicks { get; set; }
        public FileGetAdvertiseDisplayDto FileLink { get; set; }
        public List<TagGetAllDataDto> TagDatas { get; set; }
    }
}

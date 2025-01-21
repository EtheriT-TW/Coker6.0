
using EtheriT.Coker.Application.Webs.Dto;

namespace EtheriT.Coker.Application.Shared.Dto.Webs
{
    public class WebsPageDto
    {
        public int TotalPage { get; set; }
        public int PageNow { get; set; }
        public List<WebsDto> webs { get; set; }
    }
}

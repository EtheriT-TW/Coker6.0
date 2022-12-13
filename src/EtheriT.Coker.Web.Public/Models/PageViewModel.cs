using EtheriT.Coker.Application.Shared.Dto.Freight;
using EtheriT.Coker.Application.Shared.Dto.Marquee;

namespace EtheriT.Coker.Web.Public.Models
{
	public class PageViewModel
	{
		public long? id { get; set; }
		public string? search { get; set; }
        public List<FreightDisplayDto>? freightModels { get; set; }
    }
}

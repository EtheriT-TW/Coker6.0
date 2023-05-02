using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.Product;

namespace EtheriT.Coker.Web.Public.Models
{
    public class HomeViewModel
    {
        public List<HtmlContentDisplayDto>? enterAd { get; set; }
        public List<ProdGetDisplayDto>? guessLike { get; set; }
        public String site_name { get; set; }
    }
}

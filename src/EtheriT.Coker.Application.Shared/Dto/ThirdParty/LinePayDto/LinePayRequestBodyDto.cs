
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto
{
    public class LinePayRequestBodyDto
    {
        public string amount { get; set; }
        public string currency { get; set; }
        public string orderId { get; set; }
        public List<LinePayPackageDto> packages { get; set; }
        public LinePayRedirectUrlsDto redirectUrls { get; set; }
        public class LinePayRedirectUrlsDto
        {
            public string confirmUrl { get; set; }
            public string cancelUrl { get; set; }
        }
        public LinePayOptionDto options { get; set; }
    }
}


using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Application.Shared.Dto.HtmlContent
{
	public class HtmlContentDisplayDto
	{
        public string? Img { get; set; }
        public string? Content { get; set; }
        public string? Title { get; set; }
        public bool Target { get; set; }
        public string? Link { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Newsletter
{
	public class NewsletterTextUpdateDto
	{
		public long Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Conten1Title { get; set; } = string.Empty;
		public string Conten1Conten { get; set; } = string.Empty;
		public string Conten2MainTitle { get; set; } = string.Empty;
		public string Conten2Title { get; set; } = string.Empty;
		public string Conten2Conten { get; set; } = string.Empty;
	}
}

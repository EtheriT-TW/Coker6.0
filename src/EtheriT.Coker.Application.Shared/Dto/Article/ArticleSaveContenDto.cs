using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Article
{
	public class ArticleSaveContenDto
    {
        public int Id { get; set; }
        public string? SaveHtml { get; set; }
        public string? SaveCss { get; set; }
    }
}

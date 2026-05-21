using EtheriT.Coker.Application.Shared.Dto.Article;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto
{
    public class HtmlOutputDto: ArticleSaveContenDto
    {
        public string Title { get; set; } = string.Empty;
    }
}

using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Article;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Article
{
	public interface IArticleAppService
    {
        public Task<ResponseMessageDto> AddUp_Simple(ArticleDto dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<ArticleDataGetDto> GetSimple(long Id);
        public Task<ResponseMessageDto> Delete(long Id);
    }
}

using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Article
{
    public interface IArticleAppService
    {
        public Task<ResponseMessageDto> AddUp_Simple(ArticleDto dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<ArticleDataGetDto> GetSimple(long Id);
        public Task<ResponseMessageDto> Delete(long Id);
        public Task<GetArticleContenDto> GetConten(SearchIDDto dto);
        public Task<ResponseMessageDto> ImportConten(ArticleSaveContenDto dto);
        public Task<ResponseMessageDto> SaveConten(ArticleSaveContenDto dto);
        public Task<GetFrontContenOutputDto> GetFrontConten(ArticleGetFrontContenInputDto dto);
    }
}

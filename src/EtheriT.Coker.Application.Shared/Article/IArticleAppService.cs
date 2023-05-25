using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Article
{
    public interface IArticleAppService
    {
        public Task<ResponseMessageDto> AddUp(ArticleDto dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<ArticleGetDataDto> GetDataOne(long Id);
        public Task<List<DirectoryReleInfoDto>> GetDirectoryReleInfo(DirectoryReleInfoInputDto dto);
        public Task<ResponseMessageDto> Delete(long Id);
        public Task<GetArticleContenDto> GetConten(SearchIDDto dto);
        public Task<ResponseMessageDto> ImportConten(ArticleSaveContenDto dto);
        public Task<ResponseMessageDto> SaveConten(ArticleSaveContenDto dto);
        public Task<GetFrontContenOutputDto> GetFrontConten(ArticleGetFrontContenInputDto dto);
    }
}

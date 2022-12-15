using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.EnterAd;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.HtmlContent
{
    public interface IHtmlContentAppService
    {
        public Task<ResponseMessageDto> AddUp(HtmlContentDto dto);
        public Task<JsonResult> GetAllList(int type, DataSourceLoadOptions loadOptions);
        public Task<HtmlContentDto> GetOne(int id);
        public Task<ResponseMessageDto> Delete(HtmlContentDelectDto dto);
    }
}

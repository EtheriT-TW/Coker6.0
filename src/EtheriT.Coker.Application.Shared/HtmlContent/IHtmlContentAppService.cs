using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.EnterAd;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.HtmlContent
{
    public interface IHtmlContentAppService
    {
        public Task<ResponseMessageDto> AddUp(HtmlContentDto dto);
        public Task<JsonResult> GetAllList(int type, DataSourceLoadOptions loadOptions);
        public Task<HtmlContentListOutpotDto> GetAllComponent();
        public Task<HtmlContentListOutpotDto> GetComponent(long type);
        public Task<HtmlContentDto> GetOne(int id);
        public Task<JsonResult> GetDisplay(long webid, int type, int number);
        public Task<ResponseMessageDto> Delete(long Id);
        public Task<HtmlContentTypeDto> GetTypeList();
    }
}

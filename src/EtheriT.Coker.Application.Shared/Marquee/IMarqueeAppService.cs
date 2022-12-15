using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Marquee
{
    public interface IMarqueeAppService
    {
        public Task<ResponseMessageDto> AddUp(MarqueeDto dto);
        public Task<MarqueeGetDto> Get(int id);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<JsonResult> GetAll(long webid, string placement);
        public Task<ResponseMessageDto> Delete(int id);
    }
}

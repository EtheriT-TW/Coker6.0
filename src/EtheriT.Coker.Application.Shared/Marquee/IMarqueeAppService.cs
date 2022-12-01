using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Marquee
{
    public interface IMarqueeAppService
    {

        public Task<ResponseMessageDto> Add(MarqueeAddDto dto);
        public Task<ResponseMessageDto> Update(MarqueeUpdateDto dto);
        public Task<MarqueeGetDto> Get(int id);
        public Task<JsonResult> GetAll(DataSourceLoadOptions loadOptions);
        public Task<JsonResult> GetAll(long webid, string placement);
        public Task<ResponseMessageDto> Delete(int id);
    }
}

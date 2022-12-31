using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Specification
{
    public interface ISpecificationAppService
    {
        public Task<ResponseMessageDto> AddUp([FromForm] DevExpressDto dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<ResponseMessageDto> Delete(long Id);

    }
}

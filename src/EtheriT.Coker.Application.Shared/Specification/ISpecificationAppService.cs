using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Specification;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Specification
{
    public interface ISpecificationAppService
    {
        public Task<ResponseMessageDto> TypeAddUp([FromForm] DevExpressDto dto);
        public Task<ResponseMessageDto> SpecAddUp([FromForm] DevExpressDto dto);
        public Task<ResponseMessageDto> SpecAddUp(SpecSpecListDto dto);
        public Task<JsonResult> GetAllTypeList(DataSourceLoadOptions loadOptions);
        public Task<JsonResult> GetAllSpecList(DataSourceLoadOptions loadOptions);
        public Task<List<SpecTypeListDto>> GetPickTypeList();
        public Task<List<SpecTypePickListDto>> GetPickSpecList();
        public Task<ResponseMessageDto> CheckRelatSpec(long Id);
        public Task<ResponseMessageDto> TypeDelete(long Id);
        public Task<ResponseMessageDto> CheckRelatProd(long Id);
        public Task<ResponseMessageDto> SpecDelete(long Id);

    }
}

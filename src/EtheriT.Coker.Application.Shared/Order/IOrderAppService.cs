using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Order;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Order
{
    public interface IOrderAppService
    {
        public Task<ResponseMessageDto> AddHeader(OrderHeaderAddDto dto);
        public Task<ResponseMessageDto> AddDetails(OrderDetailsAddDto dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<OrderHeaderGetOneDto> GetHeaderOne(long id);
        public Task<ResponseMessageDto> Delete(int id);
        public Task<List<EnumDictionaryDto>> GetPreserveTypeEnum();
        public Task<List<EnumDictionaryDto>> GetShippingTypeEnum();
    }
}

using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Order;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EtheriT.Coker.Application.Shared.Order
{
    public interface IOrderAppService
    {
        public Task<ResponseMessageDto> AddHeader(OrderHeaderAddDto dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<OrderHeaderGetOneDto> GetHeaderOne(long id);
        public Task<List<OrderDetailsGetAllDto>> GetOrderDetails(long id);
        public Task<OrderGetDisplayDataOneDto> GetOrderDataOne(long ohid);
        public Task<OrderDataGetAllDto> GetHistoryOrder(int page);
        public Task<ResponseMessageDto> Delete(int id);
        public Task<List<EnumDictionaryDto>> GetPreserveTypeEnum();
        public Task<List<EnumDictionaryDto>> GetShippingTypeEnum();
        public Task<List<EnumDictionaryDto>> GetPaymentTypeEnum();
        public Task<ResponseMessageDto> OrderStateChange(long ohid, int state);
        public Task<ResponseMessageDto> SendMail(long ohid);
        public List<SelectDto> getOrderStatusLookup();
        public Task<ResponseMessageDto> UpdateStatus(OrderUpdateStatusDto dto);
        public Task<List<MemberOrderDto>> GetMemberOrder(Guid UUID);


    }
}

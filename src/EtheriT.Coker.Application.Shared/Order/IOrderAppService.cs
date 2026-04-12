using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Order;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Order
{
    public interface IOrderAppService
    {
        public Task<ResponseMessageDto> CheckStock(List<OrderDetailAddDto> dto);
        public Task<ResponseMessageDto> AddHeader(OrderHeaderAddDto dto);
        public Task<ResponseMessageDto> FrontUserUpdate(OrderHeaderAddDto dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<OrderHeaderGetOneDto> GetHeaderOne(long id);
        public Task<List<OrderDetailsGetAllDto>> GetOrderDetails(long id);
        public Task<List<OrderDisplayDto>> GetOrderDisplay(List<long> ohids, bool check);
        public Task<ResponseMessageDto> Reorder(long ohid);
        public Task<OrderDisplayDto> ReorderDisplay(long ohid);
        public Task<OrderDisplayDto> CheckOrder(long ohid);
        public Task<ResponseMessageDto> OrderRepay(OrderRepaySetDto dto);
        public Task<OrderDataGetAllDto> GetHistoryOrder(int page);
        public Task<ResponseMessageDto> Delete(int id);
        public Task<List<EnumDictionaryDto>> GetPreserveTypeEnum();
        public Task<List<EnumDictionaryDto>> GetShippingTypeEnum();
        public Task<List<EnumDictionaryDto>> GetPaymentTypeEnum();
        public List<SelectDto> GetFreightStatusTypeEnum();
        public Task<ResponseMessageDto> OrderStateChange(long ohid, int state);
        public Task<ResponseMessageDto> SendMail(long ohid);
        public List<SelectDto> getOrderStatusLookup();
        public Task<ResponseMessageDto> UpdateStatus(OrderUpdateStatusDto dto);
        public Task<List<MemberOrderDto>> GetMemberOrder(Guid UUID);
        public Task<ResponseMessageDto> PaySuccessMailSend(long ohid, DateTime date);
        public Task<ResponseMessageDto> PayFailMailSend(long ohid, DateTime date);
        public Task<ResponseMessageDto> CancelOrderMailSend(long ohid, DateTime date);
        public Task<ResponseMessageDto> GetForPaymentAsync(long ohid);
    }
}

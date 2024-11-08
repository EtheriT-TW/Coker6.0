using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Order;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderController : Controller
    {
        private readonly IOrderAppService orderAppService;
        public OrderController(
            IOrderAppService orderAppService
            )
        {
            this.orderAppService = orderAppService;
        }

        [HttpPost]
        public async Task<ResponseMessageDto> AddHeader(OrderHeaderAddDto dto)
        {
            return await orderAppService.AddHeader(dto);
        }

        [HttpGet]
        public async Task<OrderHeaderGetOneDto> GetHeaderOne(long id)
        {
            return await orderAppService.GetHeaderOne(id);
        }

        [HttpGet]
        public async Task<List<OrderDetailsGetAllDto>> GetOrderDetails(long id)
        {
            return await orderAppService.GetOrderDetails(id);
        }

        [HttpGet]
        public async Task<OrderDataGetAllDto> GetHistoryOrder(int page)
        {
            return await orderAppService.GetHistoryOrder(page);
        }

        [HttpPost]
        public async Task<List<EnumDictionaryDto>> GetPaymentTypeEnum()
        {
            return await orderAppService.GetPaymentTypeEnum();
        }

        [HttpGet]
        public async Task<ResponseMessageDto> CanceOrder(long ohid)
        {
            var state = (int)OrderStatusEnum.已取消;
            return await orderAppService.OrderStateChange(ohid, state);
        }
    }
}

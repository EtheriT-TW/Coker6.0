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
        private readonly IConfiguration Configuration;
        public OrderController(
            IOrderAppService orderAppService,
            IConfiguration Configuration
            )
        {
            this.orderAppService = orderAppService;
            this.Configuration = Configuration;
        }

        [HttpPost]
        public async Task<ResponseMessageDto> AddHeader(OrderHeaderAddDto dto)
        {
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            return await orderAppService.AddHeader(dto, siteId);
        }

        [HttpPost]
        public async Task<ResponseMessageDto> AddDetails(OrderDetailsAddDto dto)
        {
            return await orderAppService.AddDetails(dto);
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

        [HttpPost]
        public async Task<List<EnumDictionaryDto>> GetPaymentTypeEnum()
        {
            return await orderAppService.GetPaymentTypeEnum();
        }
    }
}

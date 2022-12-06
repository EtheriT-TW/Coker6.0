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

    }
}

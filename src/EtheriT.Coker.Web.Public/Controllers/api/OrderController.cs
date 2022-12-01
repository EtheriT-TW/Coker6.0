using DevExpress.Xpo;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Marquee;
using EtheriT.Coker.Application.Shared.Order;
using EtheriT.Coker.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;

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
        public async Task<ResponseMessageDto> Add(OrderHeaderAddDto dto)
        {
            return await orderAppService.Add(dto);
        }

    }
}

using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderAppService orderAppService;
        public OrderController(
            IOrderAppService orderAppService
            )
        {
            this.orderAppService = orderAppService;
        }

        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await orderAppService.GetAllList(loadOptions);
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
        public async Task<List<OrderDisplayDto>> GetOrderDisplay(string ohids)
        {
            List<long> list_ohid = ohids.Split(",").Where(x => long.TryParse(x, out _)).Select(long.Parse).ToList(); ;
            return await orderAppService.GetOrderDisplay(list_ohid, false);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> Delete(int id)
        {
            return await orderAppService.Delete(id);
        }

        [HttpPost]
        public async Task<List<EnumDictionaryDto>> GetPreserveTypeEnum()
        {
            return await orderAppService.GetPreserveTypeEnum();
        }

        [HttpPost]
        public async Task<List<EnumDictionaryDto>> GetShippingTypeEnum()
        {
            return await orderAppService.GetShippingTypeEnum();
        }
        [HttpPost]
        public List<SelectDto> GetFreightStatusTypeEnum()
        {
            return orderAppService.GetFreightStatusTypeEnum();
        }
        [HttpPost]
        public List<SelectDto> GetDiscountFreightTypeEnum()
        {
            return orderAppService.GetDiscountFreightTypeEnum();
        }
        [HttpGet]
        public async Task<ResponseMessageDto> SendMail(long Id)
        {
            return await orderAppService.SendMail(Id);
        }
        [HttpGet]
        public List<SelectDto> getOrderStatusLookup()
        {
            return orderAppService.getOrderStatusLookup();
        }
        [HttpPost]
        public async Task<ResponseMessageDto> UpdateStatus(OrderUpdateStatusDto dto)
        {
            return await orderAppService.UpdateStatus(dto);
        }
        [HttpGet]
        public async Task<List<MemberOrderDto>> GetMemberOrder(Guid UUID)
        {
            return await orderAppService.GetMemberOrder(UUID);
        }
        [HttpGet]

        public async Task<ResponseMessageDto> PaySuccessMailSend(long ohid, DateTime date)
        {
            return await orderAppService.PaySuccessMailSend(ohid, date);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> PayFailMailSend(long ohid, DateTime date)
        {
            return await orderAppService.PayFailMailSend(ohid, date);
        }
    }
}

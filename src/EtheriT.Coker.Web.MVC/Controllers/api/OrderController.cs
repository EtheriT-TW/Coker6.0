using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Permissions;
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
        private readonly IPermissionsAppService permissionsAppService;
        private readonly StringHandler stringHandler;
        public OrderController(
            IOrderAppService orderAppService,
            IPermissionsAppService permissionsAppService,
            StringHandler stringHandler
            )
        {
            this.orderAppService = orderAppService;
            this.permissionsAppService = permissionsAppService;
            this.stringHandler = stringHandler;
        }

        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await orderAppService.GetAllList(loadOptions);
        }
        [HttpGet]
        public async Task<OrderHeaderGetOneDto> GetHeaderOne(long id)
        {
            var order = await orderAppService.GetHeaderOne(id);
            if (order != null && !await permissionsAppService.CanViewCustomerPrivacy())
            {
                order.Orderer = stringHandler.MaskName(order.Orderer);
                order.OrdererTelePhone = stringHandler.MaskTelPhone(order.OrdererTelePhone);
                order.OrdererCellPhone = stringHandler.MaskCellPhone(order.OrdererCellPhone);
                order.OrdererEmail = stringHandler.MaskEmail(order.OrdererEmail);
                order.Recipient = stringHandler.MaskName(order.Recipient);
                order.RecipientTelePhone = stringHandler.MaskTelPhone(order.RecipientTelePhone);
                order.RecipientCellPhone = stringHandler.MaskCellPhone(order.RecipientCellPhone);
                order.RecipientAddress = stringHandler.MaskAddress(order.RecipientAddress);
                order.RecipientEmail = stringHandler.MaskEmail(order.RecipientEmail);
                order.InvoiceTitle = stringHandler.MaskName(order.InvoiceTitle);
                order.InvoiceAddress = stringHandler.MaskAddress(order.InvoiceAddress);
                order.UniformId = string.IsNullOrWhiteSpace(order.UniformId)
                    ? order.UniformId
                    : $"***{order.UniformId.Substring(Math.Max(0, order.UniformId.Length - 3))}";
                order.Carrier = string.IsNullOrWhiteSpace(order.Carrier) ? order.Carrier : "***";
            }

            return order;
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
        public async Task<ResponseMessageDto> Delete(long id)
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
        public async Task<ResponseMessageDto> SendNotificationMail(long Id)
        {
            return await orderAppService.SendNotificationMail(Id);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> SendUpdateNotificationMail(long Id)
        {
            return await orderAppService.SendUpdateNotificationMail(Id);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> ShipOrder(long Id)
        {
            return await orderAppService.ShipOrder(Id);
        }
        [HttpGet]
        public List<SelectDto> getOrderStatusLookup()
        {
            return orderAppService.getOrderStatusLookup();
        }
        [HttpPost]
        public async Task<ResponseMessageDto> UpdateStatus(OrderUpdateStatusDto dto)
        {
            dto.ForceCancel = true;
            return await orderAppService.UpdateStatus(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> UpdateLogistics(OrderUpdateLogisticsDto dto)
        {
            return await orderAppService.UpdateLogistics(dto);
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

using DevExpress.Xpo.DB.Helpers;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Order;
using EtheriT.Coker.Application.Shared.ThirdParty;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderController : Controller
    {
        private readonly IOrderAppService orderAppService;
        private readonly ILinePayAppService linePayAppService;
        private readonly IPChomePayAppService pchomePayAppService;
        private readonly IECPayAppService eCPayAppService;
        public OrderController(
            IOrderAppService orderAppService,
            ILinePayAppService linePayAppService,
            IPChomePayAppService pchomePayAppService,
            IECPayAppService eCPayAppService
            )
        {
            this.orderAppService = orderAppService;
            this.linePayAppService = linePayAppService;
            this.pchomePayAppService = pchomePayAppService;
            this.eCPayAppService = eCPayAppService;
        }

        [HttpPost]
        public async Task<ResponseMessageDto> CheckStock(List<OrderDetailAddDto> dto)
        {
            return await orderAppService.CheckStock(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> AddHeader(OrderHeaderAddDto dto)
        {
            return await orderAppService.AddHeader(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> FrontUserUpdate(OrderHeaderAddDto dto)
        {
            return await orderAppService.FrontUserUpdate(dto);
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
        public async Task<List<OrderDisplayDto>> GetOrderDisplay(long ohid, bool check)
        {
            List<long> ohids = new List<long> { ohid };
            return await orderAppService.GetOrderDisplay(ohids, check);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> Reorder(long ohid)
        {
            return await orderAppService.Reorder(ohid);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> ReorderDisplay(long ohid)
        {
            return await orderAppService.ReorderDisplay(ohid);
        }
        [HttpGet]
        public async Task<OrderDisplayDto> CheckOrder(long ohid)
        {
            return await orderAppService.CheckOrder(ohid);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> OrderRepay(OrderRepaySetDto data)
        {
            return await orderAppService.OrderRepay(data);
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
        public async Task<ResponseMessageDto> CancelOrder(long ohid, int payment)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            switch (payment)
            {
                case 1:
                    var state = (int)OrderStatusEnum.已取消;
                    response = await orderAppService.OrderStateChange(ohid, state);
                    if (response.Success && response.Message == "已付款") response.Message = "訂單已取消，請主動聯繫客服處理退款。";
                    else response.Message = "訂單已取消。";
                    break;
                case 2:
                    response = await pchomePayAppService.PChomePayCancelOrder(ohid);
                    break;
                case 3:
                    response = await linePayAppService.LinePayPayCancelOrder(ohid);
                    break;
                case 4:
                    response = await eCPayAppService.ECPayRefund(ohid);
                    break;
            }
            if (response.Message == "") response.Message = "支付方式不存在";
            return response;
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

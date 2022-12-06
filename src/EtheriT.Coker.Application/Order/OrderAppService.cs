using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.Order;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using System.Xml.Linq;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using Microsoft.AspNetCore.Http;

namespace EtheriT.Coker.Application.Order
{
    public class OrderAppService : IOrderAppService
    {
        private readonly CokerDbContext db;
        public OrderAppService(
            CokerDbContext db
        )
        {
            this.db = db;
        }
        public async Task<ResponseMessageDto> AddHeader(OrderHeaderAddDto dto)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                Core.Models.Order_Header oh = new Core.Models.Order_Header
                {
                    Orderer = dto.Orderer,
                    OrdererSex = dto.OrdererSex,
                    OrdererEmail = dto.OrdererEmail,
                    OrdererTelephone = dto.OrdererTelephone,
                    OrdererCellPhone = dto.OrdererCellPhone,
                    OrdererAddress = dto.OrdererAddress,
                    Recipient = dto.Recipient,
                    RecipientSex = dto.RecipientSex,
                    RecipientEmail = dto.RecipientEmail,
                    RecipientTelephone = dto.RecipientTelephone,
                    RecipientCellPhone = dto.RecipientCellPhone,
                    RecipientAddress = dto.RecipientAddress,
                    Remark = dto.Remark,
                    InvoiceRecipient = dto.InvoiceRecipient,
                    InvoiceTitle = dto.InvoiceTitle,
                    UniformId = dto.UniformId,
                    InvoiceAddress = dto.InvoiceAddress,
                    Shipping = dto.Shipping,
                    Payment = dto.Payment,
                    State = dto.State,
                    Total = dto.Total,
                    Discount = dto.Discount,
                    Bonus = dto.Bonus,
                    CouponId = dto.CouponId,
                    Freight = dto.Freight,
                    Service_Charge = dto.Service_Charge,
                };
                db.Order_Headers.Add(oh);
                db.SaveChanges();
                output.Success = true;
                output.Message = oh.Id.ToString();
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<ResponseMessageDto> AddDetails(OrderDetailsAddDto dto)
        {

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                /*Core.Models.Order_Header oh = new Core.Models.Order_Header
                {
                    Orderer = dto.Orderer,
                    OrdererSex = dto.OrdererSex,
                    OrdererEmail = dto.OrdererEmail,
                    OrdererTelephone = dto.OrdererTelephone,
                    OrdererCellPhone = dto.OrdererCellPhone,
                    OrdererAddress = dto.OrdererAddress,
                    Recipient = dto.Recipient,
                    RecipientSex = dto.RecipientSex,
                    RecipientEmail = dto.RecipientEmail,
                    RecipientTelephone = dto.RecipientTelephone,
                    RecipientCellPhone = dto.RecipientCellPhone,
                    RecipientAddress = dto.RecipientAddress,
                    Remark = dto.Remark,
                    InvoiceRecipient = dto.InvoiceRecipient,
                    InvoiceTitle = dto.InvoiceTitle,
                    UniformId = dto.UniformId,
                    InvoiceAddress = dto.InvoiceAddress,
                    Shipping = dto.Shipping,
                    Payment = dto.Payment,
                    State = dto.State,
                    Total = dto.Total,
                    Discount = dto.Discount,
                    Bonus = dto.Bonus,
                    CouponId = dto.CouponId,
                    Freight = dto.Freight,
                    Service_Charge = dto.Service_Charge,
                };
                db.Order_Headers.Add(oh);
                db.SaveChanges();*/
                output.Success = true;
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var result = db.Order_Headers;

                if (result != null)
                {
                    var dataQuery = from e in result
                                    where !e.IsDeleted
                                    select new OrderHeaderGetAllListDto
                                    {
                                        Id = e.Id,
                                        Orderer = e.Orderer,
                                        RecipientAddress = e.RecipientAddress,
                                        Shipping = e.Shipping,
                                        Payment = e.Payment,
                                        State = e.State,
                                        Total = e.Total,
                                        CreationTime = e.CreationTime,
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無跑馬燈資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<OrderHeaderGetAllListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<OrderHeaderGetOneDto> GetHeaderOne(long id)
        {
            try
            {
                var result = db.Order_Headers.Where(e => e.Id == id).FirstOrDefault();

                if (result != null)
                {
                    OrderHeaderGetOneDto output = new OrderHeaderGetOneDto()
                    {
                        Id = result.Id,
                        Orderer = result.Orderer,
                        OrdererTelephone = result.OrdererTelephone,
                        OrdererCellPhone = result.OrdererCellPhone,
                        Recipient = result.Recipient,
                        RecipientTelephone = result.RecipientTelephone,
                        RecipientCellPhone = result.RecipientCellPhone,
                        RecipientAddress = result.RecipientAddress,
                        InvoiceRecipient = result.InvoiceRecipient,
                        InvoiceTitle = result.InvoiceTitle,
                        UniformId = result.UniformId,
                        InvoiceAddress = result.InvoiceAddress,
                        Payment = result.Payment,
                        Total = result.Total,
                        Discount = result.Discount,
                        Bonus = result.Bonus,
                        CouponId = result.CouponId,
                        Freight = result.Freight,
                        Service_Charge = result.Service_Charge,
                    };
                    return output;
                }
                else throw new Exception("查無跑馬燈資料");
            }
            catch (Exception e)
            {

            }

            return null;
        }
        public async Task<ResponseMessageDto> Delete(int id)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var result = db.Order_Headers.Where(e => e.Id == id).FirstOrDefault();

                if (result != null)
                {
                    result.IsDeleted = true;
                    result.DeletionTime = DateTime.Now;
                    db.SaveChanges();
                    output.Success = true;
                }
                else throw new Exception("查無訂單資料");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<List<EnumDictionaryDto>> GetPreserveTypeEnum()
        {
            Dictionary<string, int> preserveTypeEnum = Enum.GetValues(typeof(PreserveTypeEnum))
                                        .Cast<PreserveTypeEnum>()
                                        .ToDictionary(k => k.ToString(), v => (int)v);

            var enumDictionaryDto = from data in preserveTypeEnum
                                    select new EnumDictionaryDto
                                    {
                                        Key = data.Key,
                                        Value = data.Value,
                                    };

            return enumDictionaryDto.ToList();
        }
        public async Task<List<EnumDictionaryDto>> GetShippingTypeEnum()
        {
            Dictionary<string, int> shippingTypeEnums = Enum.GetValues(typeof(ShippingTypeEnum))
                                        .Cast<ShippingTypeEnum>()
                                        .ToDictionary(k => k.ToString(), v => (int)v);

            var enumDictionaryDto = from data in shippingTypeEnums
                                    select new EnumDictionaryDto
                                    {
                                        Key = data.Key == "Seven取貨" ? "7-11取貨" : data.Key.Replace("_", "/"),
                                        Value = data.Value,
                                    };

            return enumDictionaryDto.ToList();
        }
    }
}

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
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
                var db_oh = db.Order_Headers.Where(e => e.Id == dto.FK_OHId).FirstOrDefault();
                if(db_oh != null)
                {
                    foreach (var scid in dto.FK_SCId_Arr)
                    {
                        var db_sc = db.ShoppingCarts.Where(e => e.Id == scid).FirstOrDefault();
                        if (db_sc != null)
                        {
                            var db_ps = db.Prod_Stocks.Where(e => e.Id == db_sc.FK_PSid).FirstOrDefault();
                            if (db_ps != null)
                            {
                                Core.Models.Order_Details od = new Core.Models.Order_Details
                                {
                                    FK_OId = db_oh.Id,
                                    FK_SCId = db_sc.Id,
                                };
                                db.Order_Details.Add(od);

                                db_sc.IsDeleted = true;
                                db_sc.DeletionTime = DateTime.Now;
                                //Core.Models.Prod_Log pl = new Core.Models.Prod_Log
                                //{

                                //    FK_Pid = db_ps.FK_Pid,
                                //    FK_Uid = user_id,
                                //    FK_Tid = dto.FK_Tid,
                                //    Action = 3,
                                //    Db_Name = "Order_Details"
                                //};
                                //db.Prod_Logs.Add(pl);

                                db.SaveChanges();
                                output.Success = true;
                            }
                            else
                            {
                                output.Success = false;
                                output.Error = "資料不存在";
                            }
                        }
                    }
                }
                else
                {
                    output.Success = false;
                    output.Error = "資料不存在";
                }

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
        public async Task<List<OrderDetailsGetAllDto>> GetDetailsOne(long id)
        {
            try
            {
                var db_oh = db.Order_Headers.Where(e => e.Id == id).FirstOrDefault();
                if(db_oh != null)
                {
                    var output = from od in db.Order_Details where od.FK_OId == db_oh.Id
                                    from sc in db.ShoppingCarts where sc.Id == od.FK_SCId
                                    from ps in db.Prod_Stocks where ps.Id == sc.FK_PSid
                                    from p in db.Prods where p.Id == ps.FK_Pid
                                    select new OrderDetailsGetAllDto
                                    {
                                        PId = p.Id,
                                        Title = p.Title,
                                        Description = p.Description,
                                        Price = p.Price,
                                        Quantity = sc.Quantity,
                                        Subtotal = p.Price * sc.Quantity
                                    };
                    return output.ToList();
                }
                else throw new Exception("查無資料");
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

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
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Application.Token;

namespace EtheriT.Coker.Application.Order
{
    public class OrderAppService : IOrderAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ITokenAppService tokenAppService;
        public OrderAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITokenAppService tokenAppService
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.tokenAppService = tokenAppService;
        }
        public async Task<ResponseMessageDto> AddHeader(OrderHeaderAddDto dto, long siteId)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                Core.Models.Order_Header oh = new Core.Models.Order_Header
                {
                    FK_WebsiteId = siteId,
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
                    Subtotal = dto.Subtotal,
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
            Guid UUID = await tokenAppService.GetUUID();
            var token = tokenAppService.CheckToken();

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            try
            {
                var db_oh = db.Order_Headers.Where(e => e.Id == dto.FK_OHId).FirstOrDefault();
                if (db_oh != null)
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

                                if (token != null)
                                {
                                    var db_t = db.Tokens.Where(e => e.id == token.RefreshToken).FirstOrDefault();
                                    if (db_t != null)
                                    {
                                        Core.Models.Prod_Log pl = new Core.Models.Prod_Log
                                        {
                                            FK_Pid = db_ps.FK_Pid,
                                            FK_UserId = db_t.UserID,
                                            UUID = UUID,
                                            Action = (int)LogActionEnum.加入訂單,
                                            Db_Name = "Order_Details"
                                        };
                                        db.Prod_Logs.Add(pl);
                                    }
                                }

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
                long WebsiteID = await loginUserData.GetWebsiteId();

                var dataQuery = from oh in db.Order_Headers
                                where !oh.IsDeleted && oh.FK_WebsiteId == WebsiteID
                                join ls in db.LogisticsSettings on oh.Shipping equals ls.Id
                                select new OrderHeaderGetAllListDto
                                {
                                    Id = ("000000000" + oh.Id.ToString()).Substring(oh.Id.ToString().Length, 9),
                                    Orderer = oh.Orderer.Substring(0, 1) + "○" + oh.Orderer.Substring(oh.Orderer.Length - 1, 1),
                                    RecipientAddress = oh.RecipientAddress.Substring(0, oh.RecipientAddress.LastIndexOf(" ")) + "***",
                                    Shipping = oh.Shipping == 0 ? ShippingTypeEnum.郵寄掛號.ToString() : ((ShippingTypeEnum)ls.LogisticsType).ToString().Replace("_", "/").Replace("Seven", "7-11"),
                                    Payment = ((PaymentTypeEnum)oh.Payment).ToString(),
                                    State = ((OrderStatusEnum)oh.State).ToString(),
                                    Total = oh.Subtotal + oh.Freight,
                                    CreationTime = oh.CreationTime,
                                };

                var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
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
                    var ship_text = "";
                    if (result.Shipping == 0)
                    {
                        ship_text = "郵寄掛號";
                    }
                    else
                    {
                        var ls = db.LogisticsSettings.Where(e => e.Id == result.Shipping).Select(e => e.LogisticsType).FirstOrDefault();
                        ship_text = ((ShippingTypeEnum)ls).ToString().Replace("_", "/").Replace("Seven", "7-11");
                    }

                    OrderHeaderGetOneDto output = new OrderHeaderGetOneDto()
                    {
                        Id = result.Id,
                        Orderer = result.Orderer,
                        OrdererTelephone = result.OrdererTelephone == null ? "-" : result.OrdererTelephone,
                        OrdererCellPhone = result.OrdererCellPhone,
                        Recipient = result.Recipient,
                        RecipientTelephone = result.RecipientTelephone == null ? "-" : result.RecipientTelephone,
                        RecipientCellPhone = result.RecipientCellPhone,
                        RecipientAddress = result.RecipientAddress.Replace(" ", ""),
                        InvoiceRecipient = result.InvoiceRecipient,
                        InvoiceTitle = result.InvoiceTitle,
                        UniformId = result.UniformId,
                        InvoiceAddress = result.InvoiceAddress,
                        Payment = ((PaymentTypeEnum)result.Payment).ToString(),
                        Shipping = ship_text,
                        State = result.State,
                        Remark = (result.Remark == "" || result.Remark == null) ? "無" : result.Remark,
                        Subtotal = result.Subtotal,
                        Total = result.Subtotal + result.Freight,
                        Discount = result.Discount,
                        Bonus = result.Bonus,
                        CouponId = result.CouponId,
                        Freight = result.Freight,
                        Service_Charge = result.Service_Charge,
                        CreationTime = result.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    return output;
                }
                else throw new Exception("查無訂單資料");
            }
            catch (Exception e)
            {

            }

            return null;
        }
        public async Task<List<OrderDetailsGetAllDto>> GetOrderDetails(long id)
        {
            try
            {
                var webSiteId = await loginUserData.GetWebsiteId();
                var orgName = await loginUserData.GetWebsiteOrgName();
                var db_oh = db.Order_Headers.Where(e => e.Id == id && e.FK_WebsiteId == webSiteId).FirstOrDefault();
                if (db_oh != null)
                {
                    var output = await (from od in db.Order_Details
                                        where od.FK_OId == db_oh.Id
                                        from sc in db.ShoppingCarts
                                        where sc.Id == od.FK_SCId
                                        from ps in db.Prod_Stocks
                                        where ps.Id == sc.FK_PSid
                                        from pp in db.Prod_Prices
                                        where !pp.IsDeleted && pp.FK_PSId == ps.Id
                                        from p in db.Prods
                                        where p.Id == ps.FK_Pid
                                        select new OrderDetailsGetAllDto
                                        {
                                            PId = p.Id,
                                            Title = p.Title,
                                            S1Title = ps.FK_S1id.ToString(),
                                            S2Title = ps.FK_S2id.ToString(),
                                            Description = p.Description,
                                            Price = pp.Price ?? 0,
                                            Quantity = sc.Quantity,
                                            Subtotal = ps.Price * sc.Quantity,
                                            ImagePath = ((from f in db.FileBinds.Include(e => e.fileUpload)
                                                  .Where(e => e.fileUpload != null && e.fileUpload.FK_WebsiteId == p.FK_WebsiteId)
                                                  .Where(e => e.fileUpload != null && !e.IsDeleted && !e.fileUpload.IsDeleted)
                                                  .Where(e => e.Sid == p.Id && e.type == (int)FileBindTypeEnum.產品)
                                                  .OrderBy(e => e.SerNo).ThenBy(e => e.CreationTime)
                                                          select new DirectoryReleInfoDto
                                                          {
                                                              Link = (f.fileUpload.DownloadFileName ?? "").Replace("upload", $"upload/{orgName}").Replace("//", "/")
                                                          }).FirstOrDefault() ?? new DirectoryReleInfoDto()).Link
                                        }).ToListAsync();

                    var db_sp = db.Prod_Specs.ToList();
                    foreach (var item in output)
                    {
                        item.S1Title = int.Parse(item.S1Title) == 0 ? "" : db_sp[int.Parse(item.S1Title) - 1].Title;
                        item.S2Title = int.Parse(item.S2Title) == 0 ? "" : db_sp[int.Parse(item.S2Title) - 1].Title;
                    }

                    return output;
                }
                else throw new Exception("查無訂單資料");
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
                long usetId = await loginUserData.GetUserId();
                var result = db.Order_Headers.Where(e => e.Id == id).FirstOrDefault();

                if (result != null)
                {
                    result.IsDeleted = true;
                    result.DeletionTime = DateTime.Now;
                    result.DeleterUserId = usetId;
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
        public async Task<List<EnumDictionaryDto>> GetPaymentTypeEnum()
        {
            Dictionary<string, int> paymentTypeEnums = Enum.GetValues(typeof(PaymentTypeEnum))
                                        .Cast<PaymentTypeEnum>()
                                        .ToDictionary(k => k.ToString(), v => (int)v);

            var enumDictionaryDto = from data in paymentTypeEnums
                                    select new EnumDictionaryDto
                                    {
                                        Key = data.Key,
                                        Value = data.Value,
                                    };

            return enumDictionaryDto.ToList();
        }
    }
}

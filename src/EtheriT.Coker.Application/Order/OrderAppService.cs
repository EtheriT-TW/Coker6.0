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
using EtheriT.Coker.Application.Token;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Dto.Mail;
using System.Globalization;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using System;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace EtheriT.Coker.Application.Order
{
    public class OrderAppService : IOrderAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ITokenAppService tokenAppService;
        private readonly IShoppingCartAppService shoppingCartAppService;
        private readonly MailAppService mailAppService;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        public OrderAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITokenAppService tokenAppService,
            IShoppingCartAppService shoppingCartAppService,
            MailAppService mailAppService,
            IConfiguration configuration,
            IMapper mapper
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.tokenAppService = tokenAppService;
            this.shoppingCartAppService = shoppingCartAppService;
            this.mailAppService = mailAppService;
            this.configuration = configuration;
            this.mapper = mapper;

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
                                    Payment = db.PaymentTypes.Where(e => e.Id == oh.Payment).Select(e => e.Title).FirstOrDefault() ?? "",
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
        public async Task<ResponseMessageDto> AddHeader(OrderHeaderAddDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                var Token = await tokenAppService.CheckToken();
                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");

                if (Token != null)
                {
                    Core.Models.Order_Header oh = mapper.Map<Order_Header>(dto);
                    oh.FK_WebsiteId = WebsiteId;
                    oh.FK_UUID = UUID;
                    oh.Fk_Tid = (Guid)Token.RefreshToken;
                    oh.Fk_UserId = await db.Tokens.Where(e => e.id == Token.RefreshToken).Select(e => e.UserID).FirstOrDefaultAsync();

                    db.Order_Headers.Add(oh);
                    db.SaveChanges();

                    if (Token.IsLogin)
                    {
                        var font_user = await db.FrontUsers.Where(e => e.UUID == UUID && e.Name == oh.Orderer && e.Email == oh.OrdererEmail).FirstOrDefaultAsync();
                        if (font_user != null)
                        {
                            if (font_user.CellPhone == null || font_user.CellPhone == "") font_user.CellPhone = oh.OrdererCellPhone;
                            if (font_user.Address == null || font_user.Address == "") font_user.Address = oh.OrdererAddress;
                            if (font_user.Sex == null) font_user.Sex = oh.OrdererSex;
                            db.SaveChanges();
                        }
                    }

                    output = await AddDetails(oh.Id);
                    if (output.Success)
                    {
                        var PaymentType = await (from pt in db.PaymentTypes
                                                 join ptv in db.PaymentTypesValues on pt.Id equals ptv.FK_PaymentTypesId
                                                 where ptv.FK_WebsiteId == WebsiteId
                                                 select pt).ToListAsync();
                        var mailoutput = await SendMail(oh.Id);
                        if (PaymentType != null)
                        {
                            switch (PaymentType.Find(e => e.Id == oh.Payment).Code)
                            {
                                case "LinePay":
                                    output.Message = $"LinePay,{oh.Id}";
                                    break;
                                default:
                                    if (PaymentType.Find(e => e.Id == oh.Payment).Code.StartsWith("PCHome") || PaymentType.Find(e => e.Id == oh.Payment).Code.StartsWith("Pchome"))
                                    {
                                        output.Message = $"PCHomePay,{oh.Id}";
                                    }
                                    else
                                    {
                                        output.Message = $"Default,{oh.Id},{oh.CreationTime.Year}年,{oh.CreationTime.Month}月{oh.CreationTime.Day + 1}日";
                                    }
                                    break;
                            }
                        }
                        if (!mailoutput.Success) output.Error = mailoutput.Message;
                    }
                }
                else throw new Exception("查無Token");
            }
            catch (Exception e)
            {
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<OrderHeaderGetOneDto> GetHeaderOne(long id)
        {
            try
            {
                var result = db.Order_Headers.Where(e => e.Id == id).FirstOrDefault();
                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId") != 0 ? configuration.GetValue<long>("WebConfig:SiteId") : await loginUserData.GetWebsiteId();

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
                        Payment = result.Payment == 0 ? "" : result.Payment.ToString(),
                        Shipping = ship_text,
                        State = result.State,
                        CompletedDate = result.CompletedDate,
                        StateStr = ((OrderStatusEnum)result.State).ToString(),
                        Remark = (result.Remark == "" || result.Remark == null) ? "無" : result.Remark,
                        Subtotal = result.Subtotal,
                        Total = result.Subtotal + result.Freight,
                        Discount = result.Discount,
                        Bonus = result.Bonus,
                        CouponId = result.CouponId,
                        Freight = result.Freight,
                        Service_Charge = result.Service_Charge,
                        CreationTime = result.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        TransactionId = result.TransactionId,
                        RefundTransactionId = result.refundTransactionId,
                        Memo = result.Memo ?? ""
                    };
                    if (output.Payment != "")
                    {
                        var payments = await (from pt in db.PaymentTypes
                                              join ptv in db.PaymentTypesValues on pt.Id equals ptv.FK_PaymentTypesId
                                              where ptv.FK_WebsiteId == WebsiteId
                                              select pt).ToListAsync();

                        if (payments.FirstOrDefault(e => e.Id == long.Parse(output.Payment)) != null)
                        {
                            var payment = payments.FirstOrDefault(e => e.Id == long.Parse(output.Payment));
                            if (payment.Code.ToLower().StartsWith("pchome"))
                            {
                                output.Payment = "支付連-" + payment.Title?.ToString() ?? "";
                            }
                            else
                            {
                                output.Payment = payment.Title?.ToString() ?? "";
                            }
                            output.ThirdParties = payment.FK_ThirdPartyId;
                        }
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
        // 改寫部分 後續會將舊程式碼移除
        private async Task<List<OrderHeaderDisplayDto>> GetHeaderDisplay(List<long> ohids, bool check)
        {
            List<OrderHeaderDisplayDto> output = new List<OrderHeaderDisplayDto>();
            try
            {
                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId") != 0 ? configuration.GetValue<long>("WebConfig:SiteId") : await loginUserData.GetWebsiteId();
                List<Order_Header> order_headers = new List<Order_Header>();
                if (check)
                {
                    var checktoken = await tokenAppService.CheckToken();
                    if (checktoken != null)
                    {
                        if (checktoken.IsLogin)
                        {
                            Guid UUID = await tokenAppService.GetUUID();
                            var uuids = await db.MappingOldNewUUID.Where(e => e.UserUUID == UUID).Select(e => e.TempUUID).ToListAsync();
                            uuids.Add(UUID);
                            var timeago = DateTime.Now.AddMinutes(-15);
                            order_headers = await db.Order_Headers.Where(e => ohids.Contains(e.Id) && uuids.Contains(e.FK_UUID) && e.CreationTime > timeago).ToListAsync();
                        }
                        else
                        {
                            order_headers = await db.Order_Headers.Where(e => ohids.Contains(e.Id) && e.Fk_Tid == checktoken.RefreshToken).ToListAsync();
                        }
                    }
                    else throw new Exception("查無Token資料");
                }
                else
                {
                    order_headers = await db.Order_Headers.Where(e => ohids.Contains(e.Id)).ToListAsync();
                }

                foreach (var order_header in order_headers)
                {
                    var temp_output = mapper.Map<OrderHeaderDisplayDto>(order_header);
                    temp_output.Subtotal = order_header.Subtotal.ToString("#,##0");
                    temp_output.Discount = (order_header.Discount ?? 0).ToString("#,##0");
                    temp_output.Bonus = (order_header.Bonus ?? 0).ToString("#,##0");
                    temp_output.CouponId = order_header.CouponId?.ToString() ?? "";
                    temp_output.Freight = order_header.Freight.ToString("#,##0");
                    temp_output.Total = (order_header.Subtotal + order_header.Freight).ToString("#,##0");
                    temp_output.StateStr = ((OrderStatusEnum)temp_output.State).ToString();

                    foreach (var property in temp_output.GetType().GetProperties())
                    {
                        var value = property.GetValue(temp_output)?.ToString();
                        if (value != null)
                        {
                            if (property.Name.EndsWith("Sex"))
                            {
                                switch (int.Parse(value))
                                {
                                    case (int)(SexEnum.男):
                                        property.SetValue(temp_output, "先生");
                                        break;
                                    case (int)(SexEnum.女):
                                        property.SetValue(temp_output, "小姐");
                                        break;
                                    case (int)(SexEnum.其他):
                                        property.SetValue(temp_output, "君");
                                        break;
                                }
                            }
                            else if (property.Name.EndsWith("Address"))
                            {
                                property.SetValue(temp_output, value.Replace(" ", ""));
                            }
                        }
                    }

                    var shipping = await db.LogisticsSettings.Where(e => e.FK_WebsiteId == WebsiteId && e.Id == order_header.Shipping).FirstOrDefaultAsync();
                    var shipping_str1 = shipping?.Title ?? "";
                    var shipping_str2 = ((PreserveTypeEnum)(shipping?.PreserveType ?? 0)).ToString();
                    var shipping_str3 = ((ShippingTypeEnum)(shipping?.LogisticsType ?? 0)).ToString().Replace("_", "/");
                    temp_output.Shipping = shipping_str1 != "" ? shipping_str2 != "" ? shipping_str3 != "" ? $"{shipping_str1}　{shipping_str2}-{shipping_str3}" : $"{shipping_str1}　{shipping_str2}" : $"{shipping_str1}" : "";
                    var payments = await (from pt in db.PaymentTypes
                                          join ptv in db.PaymentTypesValues on pt.Id equals ptv.FK_PaymentTypesId
                                          where ptv.FK_WebsiteId == WebsiteId
                                          select pt).ToListAsync();
                    if (payments.FirstOrDefault(e => e.Id == order_header.Payment) != null)
                    {
                        var payment = payments.FirstOrDefault(e => e.Id == order_header.Payment);
                        if (payment.Code.ToLower().StartsWith("pchome"))
                        {
                            temp_output.Payment = "支付連-" + payment.Title?.ToString() ?? "";
                        }
                        else
                        {
                            temp_output.Payment = payment.Title?.ToString() ?? "";
                        }
                        temp_output.ThirdParties = payment.FK_ThirdPartyId;
                    }
                    temp_output.CreationTime = order_header.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                    output.Add(temp_output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"Order=>GetHeaderDisplay回傳資料：{ex.Message}");
            }
            return output;
        }
        private async Task<ResponseMessageDto> AddDetails(long order_header_id)
        {
            Guid UUID = await tokenAppService.GetUUID();
            var Token = await tokenAppService.CheckToken();
            var userid = await db.Tokens.Where(e => e.id == Token.RefreshToken).Select(e => e.UserID).FirstOrDefaultAsync();
            var uuids = new List<Guid>();
            long role = 0;
            if (Token != null && Token.IsLogin) role = await db.MappingUserAndRoles.Where(e => e.UUID == UUID).Select(e => e.RoleId).FirstOrDefaultAsync();

            ResponseMessageDto output = new ResponseMessageDto() { Success = false };
            try
            {
                if (Token.IsLogin)
                {
                    uuids = await db.MappingOldNewUUID.Where(e => e.UserUUID == UUID && e.TempUUID != Guid.Empty).Select(e => e.TempUUID).ToListAsync();
                    uuids.Add(UUID);
                }

                var shoppingCarts = await db.ShoppingCarts.Where(e => uuids.Count == 0 ? e.FK_Tid == Token.RefreshToken : uuids.Contains(e.UUID) && !e.IsOrder).ToListAsync();

                foreach (var shoppingCart in shoppingCarts)
                {
                    var prod_stock = await db.Prod_Stocks.Where(e => e.Id == shoppingCart.FK_PSid).FirstOrDefaultAsync();

                    if (prod_stock != null)
                    {
                        Core.Models.Order_Details od = new Core.Models.Order_Details
                        {
                            FK_OId = order_header_id,
                            FK_SCId = shoppingCart.Id,
                        };
                        db.Order_Details.Add(od);

                        Core.Models.Prod_Log pl = new Core.Models.Prod_Log
                        {
                            FK_Pid = prod_stock.FK_Pid,
                            FK_UserId = userid,
                            UUID = UUID,
                            Action = (int)LogActionEnum.加入訂單,
                            Db_Name = "Order_Details"
                        };
                        db.Prod_Logs.Add(pl);

                        var db_price = await db.Prod_Prices.Where(e => e.FK_PSId == prod_stock.Id).ToListAsync();
                        if (db_price != null)
                        {
                            if (role > 0 && db_price.Find(e => e.FK_RId == role) != null)
                            {
                                shoppingCart.Price = (int)(db_price.Find(e => e.FK_RId == role).Price ?? 0);
                            }
                            else
                            {
                                shoppingCart.Price = (int)(db_price.FirstOrDefault().Price ?? 0);
                            }
                        }

                        shoppingCart.IsOrder = true;
                        shoppingCart.LastModifierUserId = userid;
                        shoppingCart.LastModificationTime = DateTime.Now;

                        db.SaveChanges();
                        output.Success = true;
                    }
                    else throw new Exception("查無商品資料");
                }
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
            return output;
        }
        public async Task<List<OrderDetailsGetAllDto>> GetOrderDetails(long id)
        {
            List<OrderDetailsGetAllDto> output = new List<OrderDetailsGetAllDto>();
            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                var db_oh = db.Order_Headers.Where(e => e.Id == id).FirstOrDefault();
                var orgName = await loginUserData.GetWebsiteOrgName();
                if (db_oh != null)
                {
                    output = await (from od in db.Order_Details
                                    where od.FK_OId == db_oh.Id
                                    from sc in db.ShoppingCarts
                                    where sc.Id == od.FK_SCId
                                    from ps in db.Prod_Stocks
                                    where ps.Id == sc.FK_PSid
                                    from pp in db.Prod_Prices
                                    where pp.FK_PSId == ps.Id && pp.FK_RId == 1
                                    from p in db.Prods
                                    where p.Id == ps.FK_Pid
                                    select new OrderDetailsGetAllDto
                                    {
                                        PId = p.Id,
                                        PSId = ps.Id,
                                        Title = p.Title,
                                        S1Title = ps.FK_S1id.ToString(),
                                        S2Title = ps.FK_S2id.ToString(),
                                        Description = p.Description,
                                        Price = sc.Price == 0 ? pp.Price ?? 0 : sc.Price,
                                        SCPrice = sc.Price,
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

                    var token = await tokenAppService.CheckToken();
                    long role = 0;
                    if (token != null && token.IsLogin) role = await db.MappingUserAndRoles.Where(e => e.UUID == UUID).Select(e => e.RoleId).FirstOrDefaultAsync();

                    var db_sp = db.Prod_Specs.ToList();
                    foreach (var item in output)
                    {
                        if (item.SCPrice == 0 && role > 1)
                        {
                            var price = await db.Prod_Prices.Where(e => e.FK_RId == role && e.FK_PSId == item.PSId).Select(e => e.Price).FirstOrDefaultAsync();
                            if (price != null && price != 0) item.Price = (double)price;
                        }
                        item.S1Title = int.Parse(item.S1Title ?? "0") == 0 ? "" : db_sp.Find(e => e.Id == int.Parse(item.S1Title!))?.Title;
                        item.S2Title = int.Parse(item.S2Title ?? "0") == 0 ? "" : db_sp.Find(e => e.Id == int.Parse(item.S2Title!))?.Title;
                    }
                }
                else throw new Exception("查無訂單資料");
            }
            catch (Exception e)
            {

            }
            return output;
        }
        // 改寫部分 後續會將舊程式碼移除
        private async Task<List<OrderDetailDisplayDto>> GetDetailsDisplay(long ohid)
        {
            List<OrderDetailDisplayDto> output = new List<OrderDetailDisplayDto>();
            var orgName = await loginUserData.GetWebsiteOrgName();
            try
            {
                var scids = await db.Order_Details.Where(e => e.FK_OId == ohid).Select(e => e.FK_SCId).ToListAsync();
                if (scids.Any())
                {
                    var shopping_carts = await shoppingCartAppService.GetDisplay(scids);
                    foreach (var shopping_cart in shopping_carts)
                    {
                        var temp_output = mapper.Map<OrderDetailDisplayDto>(shopping_cart);
                        if (orgName != "") temp_output.ImagePath = temp_output.ImagePath.Replace("/upload/", $"/upload/{orgName}/");
                        output.Add(temp_output);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"Order=>GetDetailsDisplay回傳資料：{ex.Message}");
            }
            return output;
        }
        // 改寫部分 後續會將舊程式碼移除
        public async Task<List<OrderDisplayDto>> GetOrderDisplay(List<long> ohids, bool check)
        {
            List<OrderDisplayDto> output = new List<OrderDisplayDto>();
            try
            {
                var order_headers = await GetHeaderDisplay(ohids, check);
                if (order_headers.Any())
                {
                    foreach (var order_header in order_headers)
                    {
                        var temp_output = new OrderDisplayDto();
                        temp_output.OrderHeader = order_header;
                        temp_output.OrderDetails = await GetDetailsDisplay(order_header.Id);
                        output.Add(temp_output);
                    }
                }
                else throw new Exception("查無訂單資訊");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"Order=>GetOrderDisplayOne：{ex.Message}");
            }
            return output;
        }
        public async Task<ResponseMessageDto> Reorder(long ohid)
        {
            ResponseMessageDto output = new ResponseMessageDto();

            try
            {
                var oldscids = await (from sc in db.ShoppingCarts
                                      join od in db.Order_Details on sc.Id equals od.FK_SCId
                                      join oh in db.Order_Headers on od.FK_OId equals oh.Id
                                      where oh.Id == ohid
                                      select sc.Id).ToListAsync();
                if (oldscids.Any())
                {
                    output = await shoppingCartAppService.Reorder(oldscids);
                    if (output.Success) output.Message = ohid.ToString();
                }
                else throw new Exception("查無舊訂單資訊");
            }
            catch (Exception ex)
            {
                output.Error = "Error";
                output.Message = ex.Message;
            }
            return output;
        }
        public async Task<OrderDisplayDto> ReorderDisplay(long ohid)
        {
            OrderDisplayDto output = new OrderDisplayDto();
            try
            {
                var order_headers = await GetHeaderDisplay(new List<long> { ohid }, false);
                if (order_headers.Any())
                {
                    output.OrderHeader = order_headers[0];
                    List<OrderDetailDisplayDto> order_details = new List<OrderDetailDisplayDto>();
                    List<OrderDetailDisplayDto> order_details_lock = new List<OrderDetailDisplayDto>();
                    var old_order_details = await GetDetailsDisplay(output.OrderHeader.Id);
                    var new_order_details = await shoppingCartAppService.GetAll();
                    if (old_order_details.Any())
                    {
                        if (new_order_details.Any())
                        {
                            output.OrderDetails = new List<OrderDetailDisplayDto>();
                            foreach (var old_order_detail in old_order_details)
                            {
                                if (new_order_details.Find(e => e.PSId == old_order_detail.ProdStockId) != null)
                                {
                                    var temp_new_order_detail = mapper.Map<OrderDetailDisplayDto>(new_order_details.Find(e => e.PSId == old_order_detail.ProdStockId));
                                    old_order_detail.Price = old_order_detail.Price.Replace(",", "");
                                    if (temp_new_order_detail.Price != old_order_detail.Price) temp_new_order_detail.OldPrice = old_order_detail.Price;
                                    else temp_new_order_detail.OldPrice = "0";
                                    if (temp_new_order_detail.Quantity != old_order_detail.Quantity) temp_new_order_detail.OldQuantity = old_order_detail.Quantity;
                                    output.OrderDetails.Add(temp_new_order_detail);
                                }
                                else
                                {
                                    old_order_detail.Quantity = "0";
                                    order_details_lock.Add(old_order_detail);
                                }
                            }
                            if (output.OrderDetails.Any())
                            {
                                output.OrderDetails.AddRange(order_details_lock);
                                output.Success = true;
                            }
                            else throw new Exception("查無再次下訂資訊");
                        }
                        else throw new Exception("查無再次下訂資訊");
                    }
                    else throw new Exception("查無舊訂單資訊");
                }
                else throw new Exception("查無訂單資訊");
            }
            catch (Exception ex)
            {
                output.OrderHeader = new OrderHeaderDisplayDto();
                output.Error = "Error";
                output.Message = ex.Message;
            }
            return output;
        }
        public async Task<OrderDataGetAllDto> GetHistoryOrder(int page)
        {
            var response = new OrderDataGetAllDto();
            var output = new List<OrderDataGetDto>();

            Guid UUID = await tokenAppService.GetUUID();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");

            try
            {
                var uuids = await db.MappingOldNewUUID.Where(e => e.UserUUID == UUID && e.TempUUID != Guid.Empty).Select(e => e.TempUUID).ToListAsync();
                uuids.Add(UUID);
                if (uuids.Any())
                {
                    var order_headers = await db.Order_Headers
                        .Where(e => uuids.Contains(e.FK_UUID))
                        .OrderByDescending(e => e.CreationTime).ToListAsync();

                    response.Page_Total = (int)Math.Ceiling(order_headers.Count / 8.0);
                    order_headers = order_headers.Skip((page - 1) * 8).Take(8).ToList();

                    foreach (var order_header in order_headers)
                    {
                        var temp_OrderDetails = new List<ShoppingCartGetDrop>();
                        var order_details = await db.Order_Details.Where(e => e.FK_OId == order_header.Id).ToListAsync();
                        foreach (var order_detail in order_details)
                        {
                            var shoppingCart = await db.ShoppingCarts.Where(e => e.Id == order_detail.FK_SCId && e.IsOrder).FirstOrDefaultAsync();
                            if (shoppingCart != null)
                            {
                                temp_OrderDetails.Add(await shoppingCartAppService.GetDropOne(shoppingCart.Id, true));
                            }
                        }
                        output.Add(new OrderDataGetDto()
                        {
                            OrderHeader = await GetHeaderOne(order_header.Id),
                            OrderDetails = temp_OrderDetails
                        });
                    }
                }
                response.OrderData = output;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> Delete(int id)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                long usetId = await loginUserData.GetUserId();
                var order_header = db.Order_Headers.Where(e => e.Id == id).FirstOrDefault();

                if (order_header != null)
                {
                    order_header.IsDeleted = true;
                    order_header.DeletionTime = DateTime.Now;
                    order_header.DeleterUserId = usetId;

                    var order_details = await db.Order_Details.Where(e => e.FK_OId == order_header.Id).ToListAsync();
                    if (order_details != null)
                    {
                        foreach (var order_detail in order_details)
                        {
                            order_detail.IsDeleted = true;
                            order_detail.DeletionTime = DateTime.Now;
                            order_detail.DeleterUserId = usetId;
                        }
                    }

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
        public async Task<ResponseMessageDto> OrderStateChange(long ohid, int state)
        {
            var response = new ResponseMessageDto();

            try
            {
                var order_header = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();
                DateTime now = DateTime.Now.AddDays(-7);
                if (order_header != null && order_header.State != (OrderStatusEnum)state)
                {
                    if (!(order_header.State == OrderStatusEnum.已取消 || (order_header.State == OrderStatusEnum.已完成 && order_header.CompletedDate != null && order_header.CompletedDate < now)))
                    {
                        if (order_header.State == OrderStatusEnum.已付款) response.Message = "已付款";
                        order_header.State = (OrderStatusEnum)state;
                        switch (state)
                        {
                            case (int)OrderStatusEnum.已取消:
                            case (int)OrderStatusEnum.付款失敗:
                                var shoppingCarts = await (from sc in db.ShoppingCarts
                                                           join od in db.Order_Details on sc.Id equals od.FK_SCId
                                                           where od.FK_OId == ohid && sc.IsOrder
                                                           select sc).ToListAsync();
                                if (shoppingCarts != null)
                                {
                                    var prod_stocks = new List<Prod_Stock>();
                                    foreach (var sc in shoppingCarts)
                                    {
                                        var prod_stock = await db.Prod_Stocks.Where(e => e.Id == sc.FK_PSid).FirstOrDefaultAsync();
                                        if (prod_stock != null)
                                        {
                                            prod_stock.Stock += sc.Quantity;
                                            prod_stocks.Add(prod_stock);
                                        }
                                    }
                                    db.SaveChanges();
                                }
                                break;
                            case (int)OrderStatusEnum.已完成:
                                order_header.CompletedDate = DateTime.Now;
                                break;
                        }
                        db.SaveChanges();
                        response.Success = true;
                    }
                    else throw new Exception("訂單不可更改狀態");
                }
                else throw new Exception("查無訂單資訊");
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
        public async Task<ResponseMessageDto> SendMail(long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                long WebsiteID = configuration.GetValue<long>("WebConfig:SiteId");
                if (WebsiteID == 0) WebsiteID = await loginUserData.GetWebsiteId();
                var Website = await db.Websites.Where(e => e.Id == WebsiteID).FirstOrDefaultAsync();
                var order_header = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();
                var order_details = await GetOrderDetails(ohid);

                if (order_header != null && order_details != null)
                {
                    var InvoiceRecipient = (order_header.InvoiceRecipient == 1) ? "訂購人" : (order_header.InvoiceRecipient == 2) ? "收件人" : "公司(三聯)";
                    var InvoiceTable = "";
                    if (order_header.InvoiceRecipient == 3)
                    {
                        InvoiceTable = $"<tr>" +
                                                        $"<td class='text-end'>發票抬頭</td>" +
                                                        $"<td colspan='3' class='text-start'>{order_header.InvoiceTitle}</td>" +
                                                        $"</tr>" +
                                                        $"<tr>" +
                                                        $"<td class='text-end'>統一編號</td>" +
                                                        $"<td colspan='3' class='text-start'>{order_header.UniformId}</td>" +
                                                        $"</tr>" +
                                                        $"<tr>" +
                                                        $"<td class='text-end'>寄送地址</td>" +
                                                        $"<td colspan='3' class='text-start'>{order_header.InvoiceAddress}</td>" +
                                                        $"</tr>";
                    }
                    var Shipping = await db.LogisticsSettings.Where(e => e.Id == order_header.Shipping).FirstOrDefaultAsync();
                    var PaymentType = await db.PaymentTypes.Where(e => e.FK_ThirdPartyId == order_header.Payment).Select(e => e.Title).FirstOrDefaultAsync();
                    var ThirdParty = await (from tpk in db.ThirdPartyKeypairs
                                            join tpkv in db.ThirdPartyKeypairValues on tpk.Id equals tpkv.FK_ThirdPartyKeypairId
                                            where tpk.FK_TPid == order_header.Payment
                                            where tpkv.FK_WebsiteId == WebsiteID
                                            select new ThirdPartyKeypairItemOutputDto()
                                            {
                                                Id = tpkv.Id,
                                                Title = tpk.Title,
                                                Code = tpk.Code,
                                                Value = tpkv.Value
                                            }).ToListAsync();
                    var PaymentTable = "";
                    var PaymentInfo = "";
                    if (PaymentType == "ATM")
                    {
                        PaymentTable = $"<tbody>" +
                                                        $"<tr>" +
                                                        $"<td class='text-start text-red'>您選擇的付款方式為ATM轉帳方式，目前尚未付款完成，請您於繳費期限內完成，繳費完成後請主動與公司客服聯絡。若逾期未付清款項將自動取消本訂單，謝謝。</td>" +
                                                        $"</tr>" +
                                                        $"</tbody>";
                        foreach (var data in ThirdParty)
                        {
                            PaymentInfo += $"<tr>" +
                                                                $"<td scope='row' class='text-end'>{data.Title}</td>" +
                                                                $"<td class='text-start'>{data.Value}</td>" +
                                                                $"</tr>";
                        }
                    }

                    var DetailsTable = "";
                    foreach (var data in order_details)
                    {
                        var Specification = data.S1Title != "" ? data.S2Title != "" ? $"{data.S1Title}、{data.S2Title}" : data.S1Title : "";
                        DetailsTable += $"<tr>" +
                                                        $"<td class='text-start'>{data.Title}</td>" +
                                                        $"<td>{Specification}</td>" +
                                                        $"<td>{data.Price.ToString("N", CultureInfo.CurrentCulture)}</td>" +
                                                        $"<td>{data.Quantity}</td>" +
                                                        $"<td>${(data.Price * data.Quantity).ToString("N", CultureInfo.CurrentCulture)}</td>" +
                                                        $"</tr>";
                    }

                    var OrdererEmailSecret = (order_header.OrdererEmail.Length > 5 ? order_header.OrdererEmail.Substring(0, 4) : order_header.OrdererEmail.Substring(0, 1)) + "**********";
                    order_header.OrdererCellPhone = (order_header.OrdererCellPhone.Length > 4 ? order_header.OrdererCellPhone.Substring(0, 4) : order_header.OrdererCellPhone.Substring(0, 1)) + "******";
                    order_header.OrdererTelephone = order_header.OrdererTelephone != null ? order_header.OrdererTelephone.Length > 3 ? order_header.OrdererTelephone?.Substring(0, 3) + "******" : order_header.OrdererTelephone?.Substring(0, 1) + "******" : "";
                    var OrdererSex = order_header.OrdererSex == 1 ? "先生" : order_header.OrdererSex == 2 ? "小姐" : "君";
                    order_header.RecipientAddress = order_header.RecipientAddress.Replace(" ", "").Substring(0, 6) + "**********";
                    order_header.RecipientCellPhone = (order_header.RecipientCellPhone.Length > 4 ? order_header.RecipientCellPhone.Substring(0, 4) : order_header.RecipientCellPhone.Substring(0, 1)) + "******";
                    order_header.RecipientTelephone = order_header.RecipientTelephone != null ? order_header.RecipientTelephone.Length > 3 ? order_header.RecipientTelephone?.Substring(0, 3) + "******" : order_header.RecipientTelephone?.Substring(0, 1) + "******" : "";
                    var RecipientSex = order_header.RecipientSex == 1 ? "先生" : order_header.RecipientSex == 2 ? "小姐" : "君";

                    var mailhtml = $"<div class='text-size1'><h2 class='text-red'>親愛的會員，您好！</h2>" +
                                             $"<br/>" +
                                             $"<div>非常感謝您的訂購，以下為您的訂購清單，而非付款收據，為保障您的安全，部分訊息將以'*'標記。</div>" +
                                             $"<ul>" +
                                             $"<li>若您使用ATM轉帳付款，請於訂購日起兩日內轉帳，繳費完成後請主動與公司客服聯絡。</li>" +
                                             $"<li>若您使用其他付款方式或貨到付款，您可以制定單查詢了解訂單詳情與處理進度。</li>" +
                                             $"<li>如因交易條件有誤、商品缺貨、價格誤刊或有其他本公司無法接受訂單之情形，本公司保留商品出貨與否的權利。</li>" +
                                             $"</ul>" +
                                             $"<hr/>" +
                                             $"<h2><span class='text-red'>訂單編號：</span>{("000000000" + order_header.Id).Substring((order_header.Id.ToString()).Length)}</h2>" +
                                             $"<table>" +
                                             $"<thead>" +
                                             $"<tr><th scope='col' colspan='4' class='text-start'>訂購人：{order_header.Orderer.Substring(0, 1) + "*****"} {OrdererSex}</th></tr>" +
                                             $"</thead>" +
                                             $"<tbody>" +
                                             $"<tr>" +
                                             $"<td class='text-end'>電子信箱</td>" +
                                             $"<td colspan='3' class='text-start'>{OrdererEmailSecret}</td>" +
                                             $"</tr>" +
                                             $"<tr>" +
                                             $"<td class='text-end'>手機</td>" +
                                             $"<td>{order_header.OrdererCellPhone}</td>" +
                                             $"<td class='text-end'>電話</td>" +
                                             $"<td>{order_header.OrdererTelephone}</td>" +
                                             $"</tr>" +
                                             $"</tbody>" +
                                             $"</table>" +
                                             $"<br/>" +
                                             $"<table>" +
                                             $"<thead>" +
                                             $"<tr><th scope='col' colspan='4' class='text-start'>收件人：{order_header.Recipient.Substring(0, 1) + "*****"} {RecipientSex}</th></tr>" +
                                             $"</thead>" +
                                             $"<tbody>" +
                                             $"<tr>" +
                                             $"<td class='text-end'>寄送地址</td>" +
                                             $"<td colspan='3' class='text-start'>{order_header.RecipientAddress}</td>" +
                                             $"</tr>" +
                                             $"<tr>" +
                                             $"<td class='text-end'>手機</td>" +
                                             $"<td>{order_header.RecipientCellPhone}</td>" +
                                             $"<td class='text-end'>電話</td>" +
                                             $"<td>{order_header.RecipientTelephone}</td>" +
                                             $"</tr>" +
                                             $"</tbody>" +
                                             $"</table>" +
                                             $"<br/>" +
                                             $"<table>" +
                                             $"<thead>" +
                                             $"<tr><th scope='col' colspan='4' class='text-start'>發票寄送：{InvoiceRecipient}</th></tr>" +
                                             $"</thead>" +
                                             $"<tbody>" + InvoiceTable + $"</tbody>" +
                                             $"</table>" +
                                             $"<br/>" +
                                             $"<table>" +
                                             $"<thead>" +
                                             $"<tr><th scope='col' colspan='4' class='text-start'>備註</th></tr>" +
                                             $"</thead>" +
                                             $"<tbody>" +
                                             $"<tr>" +
                                             $"<td class='text-start'>{order_header.Remark}</td>" +
                                             $"</tr>" +
                                             $"</tbody>" +
                                             $"</table>" +
                                             $"<br/>" +
                                             $"<table>" +
                                             $"<thead>" +
                                             $"<tr>" +
                                             $"<th scope='col' class='text-start'>購物明細</th>" +
                                             $"<th scope='col'>規格</th>" +
                                             $"<th scope='col'>單價</th>" +
                                             $"<th scope='col'>數量</th>" +
                                             $"<th scope='col'>小計</th>" +
                                             $"</tr>" +
                                             $"</thead>" +
                                             $"<tbody>" +
                                             DetailsTable +
                                             $"</tbody>" +
                                             $"<tfoot>" +
                                             $"<tr>" +
                                             $"<th colspan='6' class='text-end'>運費<span class='text-red ms-1 text-size1_25'>${order_header.Freight.ToString("N", CultureInfo.CurrentCulture)}</span></th>" +
                                             $"</tr>" +
                                             $"<tr>" +
                                             $"<th colspan='6' class='text-end'>消費總計<span class='text-red ms-1 text-size1_5'>${order_header.Subtotal.ToString("N", CultureInfo.CurrentCulture)}</span></th>" +
                                             $"</tr>" +
                                             $"</tfoot>" +
                                             $"</table>" +
                                             $"<br/>" +
                                             $"<table>" +
                                             $"<thead>" +
                                             $"<tr><th scope='col' class='text-start'>運送方式：<span class='text-red ms-1 text-size1_5'>{Shipping.Title}　{(PreserveTypeEnum)Shipping.PreserveType}-{(ShippingTypeEnum)Shipping.LogisticsType}</span></th></tr>" +
                                             $"</thead>" +
                                             $"</table>" +
                                             $"<br/>" +
                                             $"<table>" +
                                             $"<thead>" +
                                             $"<tr><th scope='col' class='text-start'>付款方式：<span class='text-red ms-1 text-size1_5'>{PaymentType}</span></th></tr>" +
                                             $"</thead>" +
                                             PaymentTable +
                                             $"</table>" +
                                             $"<br/>" +
                                             $"<table>" +
                                             $"<thead>" +
                                             $"<tr><th scope='col' colspan='2' class='text-start'>繳費資訊</th></tr>" +
                                             $"</thead>" +
                                             $"<tbody>" +
                                             PaymentInfo +
                                             $"<tr>" +
                                             $"<td scope='row' class='text-end'>應繳金額</td>" +
                                             $"<td class='text-start'>${order_header.Subtotal.ToString("N", CultureInfo.CurrentCulture)}</td>" +
                                             $"</tr>" +
                                             $"<tr>" +
                                             $"<td scope='row' class='text-end text-red'>繳費期限</td>" +
                                             $"<td class='text-start text-red'>{order_header.CreationTime.AddDays(1).ToString("yyyy/MM/dd")}</td>" +
                                             $"</tr>" +
                                             $"</tbody>" +
                                             $"</table>" +
                                             $"<br/>" +
                                             $"<hr/>" +
                                             $"<div class='text-bold text-red'>提醒您：此封『會員通知』為系統發出，請勿直接回覆。</div>" +
                                             $"<div class='text-bold text-red'>客服人員均不會要求消費者更改帳號或要求以ATM重新轉帳匯款</div>" +
                                             $"<div class='text-bold text-red'>若有上述情形，請立即撥打165防詐騙專線查詢</div>" +
                                             $"<hr/>" +
                                             $"</div>";
                    var mailcss = "*{ font-family: sans-serif; } .text-size1{ font-size: 1rem; } .text-size1_25{ font-size: 1.25rem; } .text-size1_5{ font-size: 1.5rem; } .text-bold {  font-weight: bold; } .text-red {  color: red; } .text-start{ text-align: start; } .text-end{ text-align: end; } .ms-1{ margin-left: 1rem; } thead{ background-color: #F2F2F2; } table { border-collapse: collapse; border: 2px solid #8c8c8c; letter-spacing: 1px; width: 600px; margin: 1rem 0 1rem 0; } th,td { border: 1px solid #a0a0a0; padding: 8px 10px; }";

                    var sedResult = await mailAppService.sendMail(new SenderDto
                    {
                        Recipients = new List<MailUserDataDto>(){
                                        new MailUserDataDto()
                                        {
                                            Name = order_header.Orderer,
                                            Email = order_header.OrdererEmail,
                                        }
                                    },
                        Subject = $"訂購通知【{Website.Title}】",
                        Body = mailhtml,
                        Css = mailcss,
                    }, WebsiteID);

                    response = sedResult;
                }
                else throw new Exception("查無訂購資料");
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }

            return response;
        }
        public List<SelectDto> getOrderStatusLookup()
        {
            return EnumHelper.EnumToKeyValueList<OrderStatusEnum>();
        }
        public async Task<ResponseMessageDto> UpdateStatus(OrderUpdateStatusDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var webSiteId = await loginUserData.GetWebsiteId();
                var order = await db.Order_Headers.Where(e => e.Id == dto.Id && e.FK_WebsiteId == webSiteId).FirstOrDefaultAsync();
                if (order != null)
                {
                    response = await OrderStateChange(order.Id, (int)dto.Status);
                    if (response.Success)
                    {
                        order.Memo = dto.Memo;
                        await loginUserData.SaveChanges(order);
                        response.Success = true;
                        await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
                    }
                }
                else throw new Exception("訂單不存在");
            }
            catch (Exception e)
            {
                response.Error = e.Message;
            }
            return response;
        }
        public async Task<List<MemberOrderDto>> GetMemberOrder(Guid UUID)
        {
            List<MemberOrderDto> output = new List<MemberOrderDto>();

            try
            {
                output = await (from oh in db.Order_Headers
                                join pt in db.PaymentTypes on oh.Payment equals pt.Id
                                where oh.FK_UUID == UUID
                                select new MemberOrderDto()
                                {
                                    Id = oh.Id,
                                    OrderDate = oh.CreationTime.ToString("g"),
                                    Payment = pt.Title == null ? "" : pt.Title,
                                    OrderTotal = oh.Subtotal.ToString("N0"),
                                    Status = ((OrderStatusEnum)oh.State).ToString(),
                                }).Take(3).ToListAsync();
            }
            catch (Exception e)
            {

            }

            return output;
        }
    }
}

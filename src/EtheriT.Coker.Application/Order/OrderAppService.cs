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
using EtheriT.Coker.Web.Core.Models;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Drawing;
using Microsoft.Extensions.Logging;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using EtheriT.Coker.Application.StoreSet;
using Microsoft.CodeAnalysis.CSharp;
using EtheriT.Coker.Application.Shared.Dto.Files;

namespace EtheriT.Coker.Application.Order
{
    public class OrderAppService : IOrderAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ITokenAppService tokenAppService;
        private readonly IShoppingCartAppService shoppingCartAppService;
        private readonly IAccountAppService accountAppService;
        private readonly IStoreSetAppService storeSetAppService;
        private readonly MailAppService mailAppService;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        public OrderAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITokenAppService tokenAppService,
            IShoppingCartAppService shoppingCartAppService,
            IAccountAppService accountAppService,
            IStoreSetAppService storeSetAppService,
            MailAppService mailAppService,
            IConfiguration configuration,
            IMapper mapper
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.tokenAppService = tokenAppService;
            this.shoppingCartAppService = shoppingCartAppService;
            this.accountAppService = accountAppService;
            this.storeSetAppService = storeSetAppService;
            this.mailAppService = mailAppService;
            this.configuration = configuration;
            this.mapper = mapper;

        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            long WebsiteID = await loginUserData.GetWebsiteId();
            string error = string.Empty;
            try
            {
                var frontuser = await (from fu in db.FrontUsers
                                       join mapfrontweb in db.MappingFrontUserAndWebsite on fu.Id equals mapfrontweb.FK_UserId
                                       where mapfrontweb.FK_WebsiteId == WebsiteID
                                       select fu).ToListAsync();

                var dataQuery = await (from oh in db.Order_Headers
                                       where !oh.IsDeleted && oh.FK_WebsiteId == WebsiteID
                                       join ls in db.LogisticsSettings on oh.Shipping equals ls.Id
                                       orderby oh.Id descending
                                       select new OrderHeaderGetAllListDto
                                       {
                                           UUID = oh.FK_UUID,
                                           Id = ("000000000" + oh.Id.ToString()).Substring(oh.Id.ToString().Length, 9),
                                           Orderer = oh.Orderer.Substring(0, 1) + "○" + oh.Orderer.Substring(oh.Orderer.Length - 1, 1),
                                           RecipientAddress = oh.RecipientAddress.Substring(0, oh.RecipientAddress.LastIndexOf(" ")) + "***",
                                           Shipping = oh.Shipping == 0 ? ShippingTypeEnum.郵寄掛號.ToString() : ((ShippingTypeEnum)ls.LogisticsType).ToString().Replace("_", "/").Replace("Seven", "7-11"),
                                           Payment = db.PaymentTypes.Where(e => e.Id == oh.Payment).Select(e => e.Title).FirstOrDefault() ?? "",
                                           State = ((OrderStatusEnum)oh.State).ToString(),
                                           Total = oh.Subtotal + oh.Freight,
                                           CreationTime = oh.CreationTime,
                                       }).ToListAsync();

                if (dataQuery.Any())
                {
                    foreach (var data in dataQuery)
                    {
                        if (data.UUID != Guid.Empty)
                        {
                            var memberid = frontuser.FirstOrDefault(e => e.UUID == data.UUID)?.Id ?? 0;
                            var memberid_str = ($"000000000{memberid}").Substring(memberid.ToString().Length);
                            data.MemberId = memberid_str;
                        }
                    }
                }
                var output = DataSourceLoader.Load(dataQuery, loadOptions);
                return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            }
            catch (Exception e)
            {
                error = e.Message;
            }
            return new JsonResult(new { error }, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<ResponseMessageDto> CheckStock(List<OrderDetailAddDto> dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var scids = dto.Select(e => e.Id).ToList();
                var scs = await db.ShoppingCarts.Where(e => scids.Contains(e.Id)).ToListAsync();
                if (scs.Count == scids.Count)
                {
                    for (int i = 0; i < scs.Count; i++)
                    {
                        if (scs[i].Quantity != dto[i].Quantity) throw new Exception("商品規格於結帳過程中發生變動");
                    }
                }
                else throw new Exception("查無購物車資料");
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = "Error";
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseMessageDto> AddHeader(OrderHeaderAddDto dto)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                var Token = await tokenAppService.CheckToken(null);
                var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
                var datetime_now = DateTime.Now;

                if (Token != null)
                {
                    Core.Models.Order_Header oh = mapper.Map<Order_Header>(dto);
                    oh.FK_WebsiteId = WebsiteId;
                    oh.FK_UUID = UUID;
                    oh.Fk_Tid = (Guid)Token.RefreshToken;
                    oh.Fk_UserId = await db.Tokens.Where(e => e.id == Token.RefreshToken).Select(e => e.UserID).FirstOrDefaultAsync();
                    oh.CreationTime = datetime_now;

                    db.Order_Headers.Add(oh);
                    db.SaveChanges();

                    List<Core.Models.Order_Details> ods = new List<Core.Models.Order_Details>();
                    List<long> scids = new List<long>();
                    foreach (var data in dto.OrderDetails)
                    {
                        scids.Add(data.Id);
                        ods.Add(new Core.Models.Order_Details()
                        {
                            FK_OId = oh.Id,
                            FK_SCId = data.Id,
                            CreationTime = datetime_now,
                        });
                    }
                    db.Order_Details.AddRange(ods);
                    db.SaveChanges();

                    List<Core.Models.ShoppingCart> scs = await db.ShoppingCarts.Where(e => scids.Contains(e.Id)).ToListAsync();
                    List<Core.Models.Prod_Log> pls = new List<Core.Models.Prod_Log>();
                    for (int i = 0; i < scs.Count; i++)
                    {
                        scs[i].Quantity = dto.OrderDetails[i].Quantity;
                        scs[i].Price = dto.OrderDetails[i].Price;
                        scs[i].IsOrder = true;
                        scs[i].LastModifierUserId = oh.CreatorUserId;
                        scs[i].LastModificationTime = datetime_now;

                        pls.Add(new Core.Models.Prod_Log()
                        {
                            FK_Pid = await db.Prod_Stocks.Where(e => e.Id == scs[i].FK_PSid).Select(e => e.FK_Pid).FirstOrDefaultAsync(),
                            FK_UserId = oh.CreatorUserId,
                            UUID = UUID,
                            Action = (int)LogActionEnum.加入訂單,
                            Db_Name = "Order",
                            CreationTime = datetime_now,
                        });
                    }
                    db.Prod_Logs.AddRange(pls);
                    db.SaveChanges();

                    output.Success = true;

                    var PaymentType = await (from pt in db.PaymentTypes
                                             join ptv in db.PaymentTypesValues on pt.Id equals ptv.FK_PaymentTypesId
                                             join tp in db.ThirdParties on pt.FK_ThirdPartyId equals tp.Id
                                             where ptv.FK_WebsiteId == WebsiteId
                                             where pt.Id == oh.Payment
                                             select tp.Title).FirstOrDefaultAsync();
                    var mailoutput = await SendMail(oh.Id);
                    if (PaymentType != null)
                    {
                        switch (PaymentType)
                        {
                            //case "轉帳":
                            default:
                                output.Message = $"Default,{oh.Id},{oh.CreationTime.ToString("yyyy-MM-dd HH:mm")}, {oh.CreationTime.Year}年<span>{oh.CreationTime.Month}月{oh.CreationTime.Day + 1}日23點59分</span>";
                                break;
                            case "支付連":
                                output.Message = $"PCHomePay,{oh.Id},{oh.CreationTime.ToString("yyyy-MM-dd HH:mm")}";
                                break;
                            case "LINE Pay":
                                output.Message = $"LinePay,{oh.Id},{oh.CreationTime.ToString("yyyy-MM-dd HH:mm")}";
                                break;
                                //case "綠界支付":
                                //    output.Message = $"ECPay,{oh.Id}";
                                //    break;
                        }
                    }
                    if (!mailoutput.Success) output.Error = mailoutput.Message;
                }
                else throw new Exception("查無Token");
            }
            catch (Exception ex)
            {
                output.Error = ex.Message;
            }

            return output;
        }
        public async Task<ResponseMessageDto> FrontUserUpdate(OrderHeaderAddDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            Guid UUID = await tokenAppService.GetUUID();

            try
            {
                var font_user = await db.FrontUsers.Where(e => e.UUID == UUID).FirstOrDefaultAsync();
                if (font_user != null)
                {
                    var userdata = mapper.Map<FrontEditUserDto>(dto);
                    userdata.Email = null;
                    response = await accountAppService.FrontUserEdit(userdata);
                }
                else throw new Exception("查無會員資料，無法儲存");
            }
            catch (Exception ex)
            {
                response.Error = "Error";
                response.Message = ex.Message;
            }
            return response;
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
                        OrdererTelePhone = result.OrdererTelePhone == null ? "-" : result.OrdererTelePhone,
                        OrdererCellPhone = result.OrdererCellPhone,
                        Recipient = result.Recipient,
                        RecipientTelePhone = result.RecipientTelePhone == null ? "-" : result.RecipientTelePhone,
                        RecipientCellPhone = result.RecipientCellPhone,
                        RecipientAddress = result.RecipientAddress.Replace(" ", ""),
                        InvoiceRecipient = result.InvoiceRecipient,
                        InvoiceTitle = result.InvoiceTitle,
                        UniformId = result.UniformId,
                        InvoiceAddress = result.InvoiceAddress,
                        Payment = result.Payment == 0 ? "" : result.Payment.ToString(),
                        PaymentCode = result.Payment,
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

                            List<long> neediconpayment = new List<long> { 2, 7, 8, 10, 11, 12, 13, 14, 16, 17, 18, 19, 20 };
                            if (neediconpayment.Contains(output.PaymentCode)) output.PaymentIcon = $"/images/paymenticon/{payment.Icons}";
                            else output.PaymentIcon = "";
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
                    var checktoken = await tokenAppService.CheckToken(null);
                    if (checktoken != null)
                    {
                        if (checktoken.IsLogin)
                        {
                            Guid UUID = await tokenAppService.GetUUID();
                            var uuids = await db.MappingOldNewUUID.Where(e => e.UserUUID == UUID).Select(e => e.TempUUID).ToListAsync();
                            uuids.Add(UUID);
                            var timeago = DateTime.Now.AddMinutes(-15);
                            order_headers = await db.Order_Headers.Where(e => ohids.Contains(e.Id) && uuids.Contains(e.FK_UUID) && (e.CreationTime > timeago || e.RepayDate > timeago)).ToListAsync();
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

                    var userdata = await db.FrontUsers.Where(e => e.UUID == order_header.FK_UUID).FirstOrDefaultAsync();
                    if (userdata != null)
                    {
                        if (order_header.OrdererTelePhone == null) order_header.OrdererTelePhone = "";
                        if (userdata.Name == order_header.Orderer && userdata.Sex == order_header.OrdererSex && userdata.Email == order_header.OrdererEmail && userdata.TelPhone == order_header.OrdererTelePhone && userdata.CellPhone == order_header.OrdererCellPhone && userdata.Address == order_header.OrdererAddress) temp_output.OrdererId = userdata.Id;
                    }

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
                    temp_output.CreationTime = order_header.CreationTime.ToString("yyyy-MM-dd HH:mm");
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
                                    where sc.Quantity > 0
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

                    var token = await tokenAppService.CheckToken(null);
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
        public async Task<OrderDisplayDto> CheckOrder(long ohid)
        {
            OrderDisplayDto output = new OrderDisplayDto();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");

            try
            {
                var ohdata = await GetHeaderDisplay(new List<long> { ohid }, false);
                if (ohdata.Any())
                {
                    if (ohdata[0].State == (int)OrderStatusEnum.付款失敗)
                    {
                        output.OrderHeader = ohdata[0];
                        var scids = await db.Order_Details.Where(e => e.FK_OId == ohid).Select(e => e.FK_SCId).ToListAsync();
                        if (scids.Any())
                        {
                            var change_details = await shoppingCartAppService.CheckStockPrice(scids);
                            if (change_details.Any())
                            {
                                var oldsubtotal = int.Parse(ohdata[0].Subtotal.Replace(",", ""));
                                ohdata[0].OldSubtotal = oldsubtotal;
                                var subtotal = oldsubtotal;
                                List<OrderDetailDisplayDto> dis_details = new List<OrderDetailDisplayDto>();
                                foreach (var change_detail in change_details)
                                {
                                    OrderDetailDisplayDto temp_detail = mapper.Map<OrderDetailDisplayDto>(change_detail);
                                    var tag = await (from tags in db.Tags
                                                     join ta in db.Tag_Associates on tags.Id equals ta.FK_TId
                                                     where tags.FK_WebsiteId == WebsiteId
                                                     where tags.Title == "售完" && ta.FK_AId == temp_detail.ProdId && ta.Type == TagAssociateTypeEnum.商品
                                                     select tags).FirstOrDefaultAsync();
                                    // 前台不會顯示Describe 此處借來放置狀態
                                    if (tag != null)
                                    {
                                        temp_detail.Describe = "商品已下架";
                                        var price = temp_detail.Price;
                                        var quantity = temp_detail.Quantity;
                                        subtotal -= price * quantity;
                                        ohdata[0].Subtotal = subtotal.ToString("#,##0");
                                        dis_details.Add(temp_detail);
                                    }
                                    else
                                    {
                                        var change = false;
                                        ohdata[0].OldSubtotal = oldsubtotal;
                                        var old_price = temp_detail.Price;
                                        var old_quantity = temp_detail.Quantity;
                                        var new_price = old_price;
                                        var new_quantity = old_quantity;
                                        var stock = await db.Prod_Stocks.Where(e => e.Id == temp_detail.ProdStockId).FirstOrDefaultAsync();
                                        if (stock != null)
                                        {
                                            if (stock.Stock == 0)
                                            {
                                                temp_detail.OldQuantity = temp_detail.Quantity;
                                                temp_detail.Quantity = 0;
                                                new_quantity = 0;
                                                temp_detail.Describe = "商品規格庫存為0";
                                                change = true;
                                            }
                                            else if (stock.Stock < temp_detail.Quantity)
                                            {
                                                temp_detail.OldQuantity = temp_detail.Quantity;
                                                temp_detail.Quantity = stock.Stock ?? 0;
                                                new_quantity = stock.Stock ?? 0;
                                                temp_detail.Describe = "商品規格庫存不足";
                                                change = true;
                                            }
                                            if (temp_detail.DynamicPrice != temp_detail.Price)
                                            {
                                                temp_detail.OldPrice = temp_detail.Price;
                                                temp_detail.Price = temp_detail.DynamicPrice;
                                                new_price = temp_detail.DynamicPrice;
                                                temp_detail.Describe = "商品規格價格更動";
                                                change = true;
                                            }
                                            if (change)
                                            {
                                                subtotal += (new_price * new_quantity) - (old_price * old_quantity);
                                                ohdata[0].Subtotal = subtotal.ToString("#,##0");
                                                temp_detail.Price = temp_detail.Price;
                                                dis_details.Add(temp_detail);
                                            }
                                        }
                                        else throw new Exception("查無商品庫存資訊");
                                    }
                                }
                                if (dis_details.Any())
                                {
                                    output.OrderDetails = dis_details;
                                    output.Message = "Change";
                                }
                                else output.Message = "NoChange";
                                output.Success = true;
                            }
                            else throw new Exception("查無詳細訂單資訊");
                        }
                        else throw new Exception("查無詳細訂單資訊");
                    }
                    else throw new Exception($"訂單狀態為{(OrderStatusEnum)ohdata[0].State}，不可重新付款");
                }
                else throw new Exception("查無訂單資訊");
            }
            catch (Exception ex)
            {
                output.Error = "Error";
                output.Message = ex.Message;
            }
            return output;
        }
        public async Task<ResponseMessageDto> OrderRepay(OrderRepaySetDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                var userid = await db.FrontUsers.Where(e => e.UUID == UUID).Select(e => e.FK_User).FirstOrDefaultAsync();
                var ohdata = await db.Order_Headers.Where(e => e.Id == dto.ohid).FirstOrDefaultAsync();
                if (ohdata != null)
                {
                    var shoppingcarts = await (from sc in db.ShoppingCarts
                                               join od in db.Order_Details on sc.Id equals od.FK_SCId
                                               where od.FK_OId == dto.ohid
                                               select sc).ToListAsync();
                    if (shoppingcarts.Any())
                    {
                        var stockids = shoppingcarts.Select(e => e.FK_PSid).ToList();
                        var stocks = await db.Prod_Stocks.Where(e => stockids.Contains(e.Id)).ToListAsync();
                        if (stocks.Any())
                        {
                            foreach (var oddata in dto.Details)
                            {
                                var scdata = shoppingcarts.Find(e => e.Id == oddata.scid);
                                var stock = stocks.Find(e => e.Id == oddata.psid);
                                if (scdata != null && stock != null)
                                {
                                    scdata.Quantity = oddata.Quantity;
                                    scdata.Price = oddata.Price;
                                    scdata.LastModifierUserId = userid;
                                    scdata.LastModificationTime = DateTime.Now;
                                    if (scdata.Quantity <= 0)
                                    {
                                        scdata.IsDeleted = true;
                                        scdata.DeletionTime = DateTime.Now;
                                        scdata.DeleterUserId = scdata.CreatorUserId;
                                    }
                                }
                                else throw new Exception("訂單詳細有誤");
                            }
                            var subtotal = 0;
                            foreach (var scdata in shoppingcarts)
                            {
                                var stock = stocks.Find(e => e.Id == scdata.FK_PSid);
                                if (stock != null)
                                {
                                    stock.Stock -= scdata.Quantity;
                                    stock.LastModifierUserId = userid;
                                    stock.LastModificationTime = DateTime.Now;
                                    subtotal += scdata.Price * scdata.Quantity;
                                }
                                else throw new Exception("查無庫存資料");
                            }
                            if (subtotal == dto.Subtotal)
                            {
                                ohdata.Subtotal = subtotal;
                                ohdata.State = OrderStatusEnum.待確認;
                                ohdata.LastModifierUserId = userid;
                                ohdata.LastModificationTime = DateTime.Now;
                                db.SaveChanges();
                                response.Success = true;
                            }
                            else throw new Exception("變更訂單發生錯誤");
                        }
                        else throw new Exception("查無庫存資料");
                    }
                    else throw new Exception("查無訂單詳細資訊");
                }
                else throw new Exception("查無訂單資訊");
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Error = "Error";
            }
            return response;
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
                                    old_order_detail.Price = old_order_detail.Price;
                                    if (temp_new_order_detail.Price != old_order_detail.Price) temp_new_order_detail.OldPrice = old_order_detail.Price;
                                    else temp_new_order_detail.OldPrice = 0;
                                    if (temp_new_order_detail.Quantity != old_order_detail.Quantity) temp_new_order_detail.OldQuantity = old_order_detail.Quantity;
                                    output.OrderDetails.Add(temp_new_order_detail);
                                }
                                else
                                {
                                    old_order_detail.Quantity = 0;
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
                        var temp_OrderDetails = new List<ShoppingCartDisplayDto>();
                        var order_details = await db.Order_Details.Where(e => e.FK_OId == order_header.Id).ToListAsync();
                        foreach (var order_detail in order_details)
                        {
                            var shoppingCart = await db.ShoppingCarts.Where(e => e.Id == order_detail.FK_SCId && e.Quantity > 0 && e.Price > 0 && e.IsOrder).FirstOrDefaultAsync();
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
                DateTime datetimenow = DateTime.Now;
                if (order_header != null)
                {
                    if (order_header.State != (OrderStatusEnum)state)
                    {
                        if (order_header.State == OrderStatusEnum.已付款) response.Message = "已付款";
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
                                order_header.CompletedDate = datetimenow;
                                break;
                        }

                        if (state == (int)OrderStatusEnum.已取消 && (order_header.State == OrderStatusEnum.待確認 || order_header.State == OrderStatusEnum.待付款))
                        {
                            order_header.TransactionId = null;
                        }
                        order_header.State = (OrderStatusEnum)state;

                        if (order_header.State == OrderStatusEnum.已取消) await CancelOrderMailSend(order_header.Id, datetimenow);

                        db.SaveChanges();
                    }
                    response.Success = true;
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
                                                        $"<td colspan='2'>發票抬頭</td>" +
                                                        $"<td colspan='4'>{order_header.InvoiceTitle}</td>" +
                                                        $"</tr>" +
                                                        $"<tr>" +
                                                        $"<td colspan='2'>統一編號</td>" +
                                                        $"<td colspan='4'>{order_header.UniformId}</td>" +
                                                        $"</tr>" +
                                                        $"<tr>" +
                                                        $"<td colspan='2'>寄送地址</td>" +
                                                        $"<td colspan='4'>{order_header.InvoiceAddress}</td>" +
                                                        $"</tr>";
                    }
                    var Shipping = await db.LogisticsSettings.Where(e => e.Id == order_header.Shipping).FirstOrDefaultAsync();
                    var PaymentType = await db.PaymentTypes.Where(e => e.Id == order_header.Payment).Select(e => e.Title).FirstOrDefaultAsync();
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
                        PaymentTable = $"<tr>" +
                                                        $"<td colspan='6' class='text-red'>您選擇的付款方式為ATM轉帳方式，目前尚未付款完成，請您於繳費期限內完成，繳費完成後請主動與公司客服聯絡。若逾期未付清款項將自動取消本訂單，謝謝。</td>" +
                                                        $"</tr>" +
                                                         $"<tr>" +
                                                         $"<td colspan='2'  scope='row' class='text-red'>繳費期限</td>" +
                                                         $"<td colspan='4'  class='text-red'>{order_header.CreationTime.AddDays(1).ToString("yyyy/MM/dd")}</td>" +
                                                         $"</tr>";
                        foreach (var data in ThirdParty)
                        {
                            PaymentInfo += $"<tr>" +
                                                                $"<td colspan='2' scope='row'>{data.Title}</td>" +
                                                                $"<td colspan='4'>{data.Value}</td>" +
                                                                $"</tr>";
                        }
                    }

                    var DetailsTable = "";
                    foreach (var data in order_details)
                    {
                        var Specification = data.S1Title != "" ? data.S2Title != "" ? $"{data.S1Title}、{data.S2Title}" : data.S1Title : "";
                        DetailsTable += $"<tr>" +
                                                        $"<td  colspan='2' class='text-start'>{data.Title}</td>" +
                                                        $"<td>{Specification}</td>" +
                                                        $"<td class='text-end'>{data.Price.ToString("$#,##0")}</td>" +
                                                        $"<td class='text-center'>{data.Quantity}</td>" +
                                                        $"<td class='text-end'>{(data.Price * data.Quantity).ToString("$#,##0")}</td>" +
                                                        $"</tr>";
                    }

                    var OrdererEmailSecret = (order_header.OrdererEmail.Length > 5 ? order_header.OrdererEmail.Substring(0, 4) : order_header.OrdererEmail.Substring(0, 1)) + "**********";
                    order_header.OrdererCellPhone = (order_header.OrdererCellPhone.Length > 4 ? order_header.OrdererCellPhone.Substring(0, 4) : order_header.OrdererCellPhone.Substring(0, 1)) + "******";
                    order_header.OrdererTelePhone = order_header.OrdererTelePhone != null ? order_header.OrdererTelePhone.Length > 3 ? order_header.OrdererTelePhone?.Substring(0, 3) + "******" : order_header.OrdererTelePhone?.Substring(0, 1) + "******" : "";
                    var OrdererSex = order_header.OrdererSex == 1 ? "先生" : order_header.OrdererSex == 2 ? "小姐" : "君";
                    order_header.RecipientAddress = order_header.RecipientAddress.Replace(" ", "").Substring(0, 6) + "**********";
                    order_header.RecipientCellPhone = (order_header.RecipientCellPhone.Length > 4 ? order_header.RecipientCellPhone.Substring(0, 4) : order_header.RecipientCellPhone.Substring(0, 1)) + "******";
                    order_header.RecipientTelePhone = order_header.RecipientTelePhone != null ? order_header.RecipientTelePhone.Length > 3 ? order_header.RecipientTelePhone?.Substring(0, 3) + "******" : order_header.RecipientTelePhone?.Substring(0, 1) + "******" : "";
                    var RecipientSex = order_header.RecipientSex == 1 ? "先生" : order_header.RecipientSex == 2 ? "小姐" : "君";

                    var mailhtml = @$"<div class='text-size1'><h2 class='text-red'>親愛的會員，您好！</h2>
                 <br/>
                 <div>非常感謝您的訂購，以下為您的訂購清單，而非付款收據，為保障您的安全，部分訊息將以'*'標記。</div>
                 <ul>
                 <li>若您使用ATM轉帳付款，請於訂購日起兩日內轉帳，繳費完成後請主動與公司客服聯絡。</li>
                 <li>若您使用其他付款方式或貨到付款，您可以制定單查詢了解訂單詳情與處理進度。</li>
                 <li>如因交易條件有誤、商品缺貨、價格誤刊或有其他本公司無法接受訂單之情形，本公司保留商品出貨與否的權利。</li>
                 </ul>
                 <hr/>
                 <h2><span class='text-red'>訂單編號：</span>{("000000000" + order_header.Id).Substring((order_header.Id.ToString()).Length)}</h2>
                 <div class=''>下單時間：{order_header.CreationTime.ToString("yyyy/MM/dd HH:mm")}</div>
                 <table>
                 <tbody>
                 <tr class='thead'><td scope='col' colspan='6'>訂購人：{order_header.Orderer.Substring(0, 1) + "*****"} {OrdererSex}</td></tr>
                 <tr>
                 <td colspan='1'>電子信箱</td>
                 <td colspan='5'>{OrdererEmailSecret}</td>
                 </tr>
                 <tr>
                 <td colspan='1'>手機</td>
                 <td colspan='2'>{order_header.OrdererCellPhone}</td>
                 <td colspan='1''>電話</td>
                 <td colspan='2'>{order_header.OrdererTelePhone}</td>
                 </tr>
                 <tr class='thead'><td scope='col' colspan='6'>收件人：{order_header.Recipient.Substring(0, 1) + "*****"} {RecipientSex}</td></tr>
                 <tr>
                 <td colspan='1'>寄送地址</td>
                 <td colspan='5'>{order_header.RecipientAddress}</td>
                 </tr>
                 <tr>
                 <td colspan='1'>手機</td>
                 <td colspan='2'>{order_header.RecipientCellPhone}</td>
                 <td colspan='1'>電話</td>
                 <td colspan='2'>{order_header.RecipientTelePhone}</td>
                 </tr>
                 <tr class='thead'><td scope='col' colspan='6'>發票寄送：{InvoiceRecipient}</td></tr>
                 {InvoiceTable}
                 <tr class='thead'><td scope='col' colspan='6'>備註</td></tr>
                 <tr>
                 <td colspan='6'>{order_header.Remark}</td>
                 </tr>
                 <tr class='thead'>
                 <td scope='col' colspan='2' class='text-center'>購物明細</td>
                 <td scope='col' class='text-center'>規格</td>
                 <td scope='col' class='text-center'>單價</td>
                 <td scope='col' class='text-center'>數量</td>
                 <td scope='col' class='text-center'>小計</td>
                 </tr>
                 {DetailsTable}
                 <tr>
                 <td colspan='6' class='text-end text-bold'>運費<span class='text-red ms-1 text-size1_25'>{order_header.Freight.ToString("$#,##0")}</span></td>
                 </tr>
                 <tr>
                 <td colspan='6' class='text-end text-bold'>消費總計<span class='text-red ms-1 text-size1_5'>{(order_header.Freight + order_header.Subtotal).ToString("$#,##0")}</span></td>
                 </tr>
                 <tr class='thead'><td colspan='6' scope='col'>運送方式：<span class='text-red ms-1 text-size1_5'>{Shipping.Title}　{(PreserveTypeEnum)Shipping.PreserveType}-{(ShippingTypeEnum)Shipping.LogisticsType}</span></td></tr>
                 <tr class='thead'><td colspan='6'  scope='col'>付款方式：<span class='text-red ms-1 text-size1_5'>{PaymentType}</span></td></tr>
                 {PaymentTable}
                 <tr class='thead'><td scope='col' colspan='6'>繳費資訊</td></tr>
                 {PaymentInfo}
                 <tr>
                 <td colspan='2' scope='row''>應繳金額</td>
                 <td colspan='4'>{(order_header.Freight + order_header.Subtotal).ToString("$#,##0")}</td>
                 </tr>
                 </tbody>
                 </table>
                 <br/>
                 <hr/>
                 <div class='text-bold text-red'>提醒您：此封『訂購通知』為系統發出，請勿直接回覆。</div>
                 <div class='text-bold text-red'>客服人員均不會要求消費者更改帳號或要求以ATM重新轉帳匯款。</div>
                 <div class='text-bold text-red'>若有上述情形，請立即撥打165防詐騙專線查詢。</div>
                 <hr/>
                 </div>";
                    var mailcss = "*{ font-family: sans-serif; } .text-size1{ font-size: 1rem; } .text-size1_25{ font-size: 1.25rem; } .text-size1_5{ font-size: 1.5rem; } .text-bold {  font-weight: bold; } .text-red {  color: red; } .text-center{ text-align: center; } .text-end{ text-align: right; } .ms-1{ margin-left: 1rem; } .thead{ background-color: #F2F2F2; font-weight: bold; } table { border-collapse: collapse; border: 2px solid #8c8c8c; letter-spacing: 1px; width: 600px; margin: 1rem 0 1rem 0; } th,td { border: 1px solid #a0a0a0; padding: 8px 10px; }";

                    MailUserDataDto cc = new MailUserDataDto { Name = "客服信箱" };
                    var conpny = await (from c in db.MappingCompanyAndWebsites.Include(e => e.Company).Where(e => e.FK_WebsiteId == WebsiteID)
                                        select c.Company).FirstOrDefaultAsync();
                    if (conpny != null)
                    {
                        cc.Email = conpny.Email;
                    }
                    else
                    {
                        var smtp = await storeSetAppService.getValues(new Shared.Dto.StoreSet.StoreSetGetValueInput { SiteId = WebsiteID, key = "SMTPAccount" });
                        if (smtp != null && smtp.Success && smtp.detailItem != null && smtp.detailItem.value != null && smtp.detailItem.value.Count > 0)
                        {
                            cc.Email = smtp.detailItem.value[0];
                        }
                    }
                    var sendDto = new SenderDto
                    {
                        Recipients = new List<MailUserDataDto>(){
                                new MailUserDataDto()
                                {
                                    Name = order_header.Orderer,
                                    Email = order_header.OrdererEmail,
                                }
                            },
                        Subject = $"【{Website.Title}】訂購通知",
                        Body = mailhtml,
                        Css = mailcss,
                    };
                    if (!string.IsNullOrEmpty(cc.Email)) sendDto.Bcc.Add(cc);

                    var sedResult = await mailAppService.sendMail(sendDto, Website.Contact);

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
        public async Task<ResponseMessageDto> PaySuccessMailSend(long ohid, DateTime date)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                long WebsiteID = configuration.GetValue<long>("WebConfig:SiteId") == 0 ? await loginUserData.GetWebsiteId() : configuration.GetValue<long>("WebConfig:SiteId");
                var Website = await db.Websites.Where(e => e.Id == WebsiteID).FirstOrDefaultAsync();
                var order_header = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();

                if (order_header != null)
                {
                    var PaymentType = await db.PaymentTypes.Where(e => e.Id == order_header.Payment).FirstOrDefaultAsync();
                    var ThirdParty = await db.ThirdParties.Where(e => e.Id == PaymentType.FK_ThirdPartyId).FirstOrDefaultAsync();
                    var Payment = PaymentType?.Title ?? "";
                    var ThirdParty_Content = "";
                    switch (ThirdParty?.Id)
                    {
                        case 1:
                            Payment = "ATM轉帳";
                            ThirdParty_Content = $"<div>感謝您使用<span class='text-bold' style='margin: 0px 5px;'>{Payment}</span>方式進行付款</div>";
                            break;
                        case 2:
                            if (PaymentType?.Code == "PchomePayCARD")
                            {
                                Payment = "信用卡一次付清(信用卡)";
                            }
                            else if (PaymentType?.Code?.StartsWith("PchomePayInstallment") ?? false)
                            {
                                Payment += "(信用卡)";
                            }
                            ThirdParty_Content = $"<div>感謝您使用<span class='text-bold' style='margin: 0px 5px;'>{ThirdParty?.Title ?? ""}</span>平台進行付款</div>";
                            break;
                        case 3:
                            ThirdParty_Content = $"<div>感謝您使用<span class='text-bold' style='margin: 0px 5px;'>{ThirdParty?.Title.Replace(" ", "") ?? ""}</span>平台進行付款</div>";
                            break;
                    }

                    var mailhtml = $@"<div class='text-size1'><h2 class='text-red'>親愛的會員，您好！</h2>
                                                            <br/>
                                                             <div>您於【{Website?.Title ?? ""}】進行了{Payment}交易，以下為您的付款完成資訊：</div>
                                                            <br/>
                                                            <table>
                                                                <tbody>
                                                                    <tr>
                                                                        <td scope='row' class='text-bold' style='text-align: center; background-color: #F2F2F2;'>商店訂單編號</td>
                                                                        <td>{("000000000" + order_header.Id).Substring((order_header.Id.ToString()).Length)}</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td scope='row' class='text-bold' style='text-align: center; background-color: #F2F2F2;'>{ThirdParty?.Title.Replace(" ", "") ?? ""}交易序號</td>
                                                                        <td>{order_header.TransactionId}</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td scope='row' class='text-bold' style='text-align: center; background-color: #F2F2F2;'>訂單金額</td>
                                                                        <td>{(order_header.Freight + order_header.Subtotal).ToString("$#,##0")}</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td scope='row' class='text-bold' style='text-align: center; background-color: #F2F2F2;'>支付方式</td>
                                                                        <td>{Payment}</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td scope='row' class='text-bold' style='text-align: center; background-color: #F2F2F2;'>付款結果</td>
                                                                        <td>付款成功</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td scope='row' class='text-bold' style='text-align: center; background-color: #F2F2F2;'>付款完成日期</td>
                                                                        <td>{date.ToString("yyyy/MM/dd HH:mm")}</td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>
                                                            <br/>
                                                             {ThirdParty_Content}
                                                             <div class='text-bold text-red'>貼心提醒：若欲詢問如商品資訊、商品出貨進度或退貨退款問題，請您與原訂購商店/網站聯繫。</div>
                                                            <br/>
                                                             <div class='text-bold text-red'>以上為您實際付款資訊，若您接獲自稱商店通知交易有「信用卡誤設分期付款」或「連續扣款」或「中獎通知」或「需加入LINE等社群帳號要求核對資料」等問題，皆為詐騙手法，請小心勿受騙上當，以免被有心詐騙者利用。</div>
                                                            <br/>
                                                             <hr/>
                                                             <div class='text-bold text-red'>提醒您：此封『付款完成通知』為系統發出，請勿直接回覆。</div>
                                                             <div class='text-bold text-red'>客服人員均不會要求消費者更改帳號或要求以ATM重新轉帳匯款。</div>
                                                             <div class='text-bold text-red'>若有上述情形，請立即撥打165防詐騙專線查詢。</div>
                                                             <hr/>
                                                        </div>";
                    var mailcss = "*{ font-family: sans-serif; } .text-size1{ font-size: 1rem; } .text-bold {  font-weight: bold; } .text-red {  color: red; } table { border-collapse: collapse; border: 2px solid #8c8c8c; letter-spacing: 1px; width: 600px; margin: 1rem 0 1rem 0; } th,td { border: 1px solid #a0a0a0; padding: 8px 10px; }";

                    if (ThirdParty?.Id == 2 && (PaymentType?.Code == "PchomePayCARD" || (PaymentType?.Code?.StartsWith("PchomePayInstallment") ?? false))) Payment = "信用卡付款";

                    var sedResult = await mailAppService.sendMail(new SenderDto
                    {
                        Recipients = new List<MailUserDataDto>(){
                                        new MailUserDataDto()
                                        {
                                            Name = order_header.Orderer,
                                            Email = order_header.OrdererEmail,
                                        }
                                    },
                        Subject = $"【{Website.Title}】付款完成通知信({Payment})",
                        Body = mailhtml,
                        Css = mailcss,
                    }, Website.Contact);

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
        public async Task<ResponseMessageDto> PayFailMailSend(long ohid, DateTime date)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            return response;
        }
        public async Task<ResponseMessageDto> CancelOrderMailSend(long ohid, DateTime date)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                long WebsiteID = configuration.GetValue<long>("WebConfig:SiteId") == 0 ? await loginUserData.GetWebsiteId() : configuration.GetValue<long>("WebConfig:SiteId");
                var Website = await db.Websites.Where(e => e.Id == WebsiteID).FirstOrDefaultAsync();
                var order_header = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();

                if (order_header != null)
                {
                    var PaymentType = await db.PaymentTypes.Where(e => e.Id == order_header.Payment).FirstOrDefaultAsync();
                    var ThirdParty = await db.ThirdParties.Where(e => e.Id == PaymentType.FK_ThirdPartyId).FirstOrDefaultAsync();
                    var Payment = PaymentType?.Title ?? "";

                    var RefundTransactionId = "";
                    var AmountTitle = "訂單金額";
                    var Remind = "";
                    var MailTitle = "取消訂單通知";
                    if (order_header.refundTransactionId != null)
                    {
                        RefundTransactionId = $@"<tr>
                                                                        <td scope='row' class='text-bold' style='text-align: center; background-color: #F2F2F2;'>{ThirdParty?.Title.Replace(" ", "") ?? ""}退款序號</td>
                                                                        <td>{order_header.refundTransactionId}</td>
                                                                    </tr>";
                        AmountTitle = "退款金額";
                        Remind = "若欲詢問退貨退款相關問題，請您與原訂購商店/網站聯繫。";
                        var RefundText = PaymentType.RefundWorkDay < 0 ? "如有貨款需退回，請聯繫原訂購商店/網站聯繫。" : PaymentType.RefundWorkDay == 0 ? $"貨款將已成功退回，{Remind}" : $"貨款將在{PaymentType.RefundWorkDay}個工作天內退回，{Remind}";
                        Remind = $"<div class='text-bold text-red'>貼心提醒：{RefundText}</div>";
                        MailTitle = "退款通知";
                    }

                    var mailhtml = $@"<div class='text-size1'><h2 class='text-red'>親愛的會員，您好！</h2>
                                                            <br/>
                                                             <div>您於【{Website?.Title ?? ""}】有筆訂單已取消，以下為取消訂單資訊：</div>
                                                            <br/>
                                                            <table>
                                                                <tbody>
                                                                    <tr>
                                                                        <td scope='row' class='text-bold' style='text-align: center; background-color: #F2F2F2;'>商店訂單編號</td>
                                                                        <td>{("000000000" + order_header.Id).Substring((order_header.Id.ToString()).Length)}</td>
                                                                    </tr>
                                                                    {RefundTransactionId}
                                                                    <tr>
                                                                        <td scope='row' class='text-bold' style='text-align: center; background-color: #F2F2F2;'>{AmountTitle}</td>
                                                                        <td>{(order_header.Freight + order_header.Subtotal).ToString("$#,##0")}</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td scope='row' class='text-bold' style='text-align: center; background-color: #F2F2F2;'>支付方式</td>
                                                                        <td>{Payment}</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td scope='row' class='text-bold' style='text-align: center; background-color: #F2F2F2;'>取消訂單日期</td>
                                                                        <td>{date.ToString("yyyy/MM/dd HH:mm")}</td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>
                                                            <br/>
                                                             {Remind}
                                                            <br/>
                                                             <hr/>
                                                             <div class='text-bold text-red'>提醒您：此封『{MailTitle}』為系統發出，請勿直接回覆。</div>
                                                             <div class='text-bold text-red'>客服人員均不會要求消費者更改帳號或要求以ATM重新轉帳匯款。</div>
                                                             <div class='text-bold text-red'>若有上述情形，請立即撥打165防詐騙專線查詢。</div>
                                                             <hr/>
                                                        </div>";
                    var mailcss = "*{ font-family: sans-serif; } .text-size1{ font-size: 1rem; } .text-bold {  font-weight: bold; } .text-red {  color: red; } table { border-collapse: collapse; border: 2px solid #8c8c8c; letter-spacing: 1px; width: 600px; margin: 1rem 0 1rem 0; } th,td { border: 1px solid #a0a0a0; padding: 8px 10px; }";

                    var sedResult = await mailAppService.sendMail(new SenderDto
                    {
                        Recipients = new List<MailUserDataDto>(){
                                        new MailUserDataDto()
                                        {
                                            Name = order_header.Orderer,
                                            Email = order_header.OrdererEmail,
                                        }
                                    },
                        Subject = $"【{Website.Title}】{MailTitle}信",
                        Body = mailhtml,
                        Css = mailcss,
                    }, Website.Contact);

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
    }
}

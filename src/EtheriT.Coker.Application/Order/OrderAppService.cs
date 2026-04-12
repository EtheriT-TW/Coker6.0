using AutoMapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Order.Dto;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using EtheriT.Coker.Application.Shared.Dto.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Bonus;
using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;
using EtheriT.Coker.Application.Shared.Dto.enumType.Order;
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Mail;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto;
using EtheriT.Coker.Application.Shared.Order;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;

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
        private readonly IBonusManagementAppService bonusManagementAppService;
        private readonly IFileUploadAppService fileUploadAppService;
        private readonly IMapper mapper;
        public OrderAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITokenAppService tokenAppService,
            IShoppingCartAppService shoppingCartAppService,
            IAccountAppService accountAppService,
            IStoreSetAppService storeSetAppService,
            IBonusManagementAppService bonusManagementAppService,
            MailAppService mailAppService,
            IConfiguration configuration,
            IFileUploadAppService fileUploadAppService,
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
            this.bonusManagementAppService = bonusManagementAppService;
            this.fileUploadAppService = fileUploadAppService;
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

                var dataQuery = await (from oh in db.Order_Headers.Include(e => e.LogisticsSetting)
                                       where !oh.IsTemp && oh.FK_WebsiteId == WebsiteID
                                       join ls in db.LogisticsSettings on oh.Shipping equals ls.Id
                                       orderby oh.Id descending
                                       select new OrderHeaderGetAllListDto
                                       {
                                           UUID = oh.FK_UUID,
                                           Id = ("000000000" + oh.Id.ToString()).Substring(oh.Id.ToString().Length, 9),
                                           Orderer = string.IsNullOrEmpty(oh.Orderer) ? "" :
                                                oh.Orderer.Length == 1 ? oh.Orderer + "○" :
                                                    oh.Orderer.Substring(0, 1) + "○" + oh.Orderer.Substring(oh.Orderer.Length - 1, 1),
                                           RecipientAddress = string.IsNullOrEmpty(oh.RecipientAddress) || !oh.RecipientAddress.Contains(" ") ?
                                                oh.RecipientAddress :
                                                oh.RecipientAddress.Substring(0, oh.RecipientAddress.LastIndexOf(" ")) + "***",
                                           Shipping = oh.Shipping == 0 ? ShippingTypeEnum.郵寄掛號.ToString() : oh.LogisticsSetting.Title,
                                           Payment = db.PaymentTypes.Where(e => e.Id == oh.Payment).Select(e => e.Title).FirstOrDefault() ?? "",
                                           State = oh.State.ToString(),
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
                await CheckECPayExpiredOrders(false);
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
            var output = new ResponseMessageDto() { Success = false };

            try
            {
                // === 共用 context ===
                var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
                var token = await tokenAppService.CheckToken(null) ?? throw new Exception("查無Token");
                var uuid = await tokenAppService.GetUUID();          // 如果實際型別是 string 就改掉
                var now = DateTime.Now;
                var userId = await db.Tokens
                        .Where(e => e.id == token.RefreshToken)
                        .Select(e => e.UserID)
                        .FirstOrDefaultAsync();
                bool isTemp = dto.IsTemp;

                DetailBuildResult? detailResult = null;

                var bonusOk = await shoppingCartAppService.checkBonusCanUse(uuid, dto.OrderDetails);
                if (!bonusOk) throw new Exception("紅利點數不足，請重新確認後再送出訂單。");

                // 1) 正式訂單才需要檢查購物車 / 庫存 / 計算金額
                if (!isTemp)
                {
                    detailResult = await BuildDetailSectionAsync(dto, userId, uuid, now);
                }

                Order_Header header = null!;
                var strategy = db.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await db.Database.BeginTransactionAsync();
                    // 2) 建立 / 更新訂單頭（head 區）
                    header = await BuildHeaderSectionAsync(dto, websiteId, uuid, token, userId, now, isTemp, detailResult);

                    // 3) 建立 Log 區資料（只 new，不存 DB）
                    var logs = BuildLogSection(header, detailResult, uuid);

                    // 4) PaySelect 區：目前你專案還沒用到，可以先空實作
                    // var paySelects = BuildPaySelectSection(header, dto);

                    // 5) 真正儲存所有資料（detail + cart + stock + logs）
                    await SaveOrderAsync(header, detailResult, logs, now, userId, isTemp);

                    await tx.CommitAsync();
                });

                // 6) Commit 後，處理付款訊息 + 寄信（失敗也不要 rollback 訂單）
                await FillPaymentMessageAndSendMailAsync(dto, websiteId, header!, output);

                output.Success = true;
            }
            catch (Exception ex)
            {
                output.Error = ex.Message;
            }

            return output;
        }
        private async Task<DetailBuildResult> BuildDetailSectionAsync(
            OrderHeaderAddDto dto,
            long? userId,
            Guid uuid,
            DateTime now)
        {
            if (dto.OrderDetails == null || dto.OrderDetails.Count == 0)
                throw new Exception("沒有任何購買項目，無法建立訂單。");

            // 只信「有哪些購物車 Id 要結帳」
            var cartIds = dto.OrderDetails.Select(d => d.Id).ToList();

            // 1) 把購物車 + 庫存 + 價格資訊一次從 DB 撈出來
            var carts = await db.ShoppingCarts
                .Include(sc => sc.Prod_Stock)
                    .ThenInclude(ps => ps.Prod)
                .Include(sc => sc.Prod_Price)
                .Where(sc =>
                    cartIds.Contains(sc.Id) &&
                    sc.UUID == uuid &&
                    !sc.IsOrder)                      // 只允許未結帳的購物車
                .ToListAsync();

            if (carts.Count == 0)
                throw new Exception("購物車已失效或無任何品項，無法建立訂單。");

            if (carts.Count != cartIds.Count)
                throw new Exception("購物車資料有誤，請重新整理後再嘗試。");

            // 建立 Stock 快取
            var stockDict = carts
                .Select(sc => sc.Prod_Stock)
                .Where(ps => ps != null)
                .ToDictionary(ps => ps.Id);

            decimal subtotal = 0;
            decimal totalBonus = 0;

            foreach (var sc in carts)
            {
                if (!stockDict.TryGetValue(sc.FK_PSid, out var stock) || stock == null)
                    throw new Exception($"找不到商品庫存資料（購物車ID={sc.Id}）。");

                var currentStock = stock.Stock ?? 0;
                var qty = sc.Quantity;    // ✅ 完全用 DB 裡的數量，不信 request

                if (currentStock < qty)
                    throw new Exception($"商品庫存不足（購物車ID={sc.Id}），剩餘 {currentStock}，欲購買 {qty}。");

                // 2) 以 Prod_Price 為準，決定單價與紅利
                decimal unitPrice;
                int unitBonus;

                if (sc.Prod_Price != null)
                {
                    unitPrice = (decimal)(sc.Prod_Price.Price ?? 0);
                    unitBonus = sc.Prod_Price.Bonus ?? 0;
                }
                else
                {
                    // 備援：若沒有綁 Prod_Price，就用購物車裡當時的快照
                    unitPrice = (decimal)sc.Price;
                    unitBonus = sc.Bonus ?? 0;
                }

                decimal lineAmount = unitPrice * qty;
                int lineBonus = unitBonus * qty;

                subtotal += lineAmount;
                totalBonus += lineBonus;

                // 3) 在記憶體中同步購物車資料為「標準答案」
                sc.Price = unitPrice;
                sc.Bonus = unitBonus;
                sc.IsOrder = true;
                sc.LastModifierUserId = userId;
                sc.LastModificationTime = now;

                // 4) 庫存扣除（同樣只是記憶體變動，之後一起 Save）
                stock.Stock = currentStock - qty;
                stock.LastModifierUserId = userId;
                stock.LastModificationTime = now;
            }
            // 5) 訂單紅利抵扣
            var bonusSetting = await bonusManagementAppService.GetBonusSettingForEdit();
            if (bonusSetting != null && bonusSetting.MaxRedemptionPercent != null && bonusSetting.MaxRedemptionPercent > 0)
            {
                var userBonus = (await bonusManagementAppService.GetQueryFrontUsersTotalAvaliableBonus(new List<Guid> { uuid })).FirstOrDefault();
                if (userBonus != null)
                {
                    var memberBonusAmount = Math.Max(0, userBonus.TotalAvaliableBonus - totalBonus);
                    if (memberBonusAmount > 0)
                    {
                        var bonusDiscount = Math.Floor(subtotal * bonusSetting.MaxRedemptionPercent.Value / 100);
                        var canRedeem = Math.Min(bonusDiscount, memberBonusAmount);
                        subtotal -= canRedeem;
                        totalBonus += (int)canRedeem;
                    }
                }
            }

            return new DetailBuildResult
            {
                ShoppingCarts = carts,
                StockDict = stockDict,
                Subtotal = (int)Math.Round(subtotal, MidpointRounding.AwayFromZero),
                TotalBonus = (int)Math.Round(totalBonus, MidpointRounding.AwayFromZero)
            };
        }
        private async Task<Order_Header> BuildHeaderSectionAsync(
            OrderHeaderAddDto dto,
            long websiteId,
            Guid uuid,                  // 若你實際不是 Guid，這邊改掉即可
            dynamic token,              // 這裡用 dynamic 是為了不跟你實際型別打架
            long? userId,
            DateTime now,
            bool isTemp,
            DetailBuildResult? detailResult)
        {
            Order_Header? oh;
            // 修改暫存單訂單為正式訂單
            if (dto.OrderId != null)
            {
                oh = await db.Order_Headers
                    .FirstOrDefaultAsync(e => e.Id == dto.OrderId && e.IsTemp);
                if (oh == null)
                    throw new Exception("找不到對應的暫存訂單。");

                mapper.Map(dto, oh);

                oh.FK_WebsiteId = websiteId;
                oh.FK_UUID = uuid;
                oh.Fk_Tid = token.RefreshToken;
                oh.Fk_UserId = userId;
                oh.CreationTime = now;

                // 正式單用我們自己算的 subtotal 蓋掉前端
                if (!isTemp && detailResult != null)
                {
                    oh.Subtotal = detailResult.Subtotal;
                    oh.Bonus = detailResult.TotalBonus;
                    if (detailResult.TotalBonus > 0)
                    {
                        var bonusResult = await bonusManagementAppService.SaveTransaction(new CreateUserTransactionDto
                        {
                            IsSendMail = false,
                            TransactionOperation = "-",
                            MemberUUID = new List<Guid> { uuid },
                            TransactionPoint = detailResult.TotalBonus,
                            TransactionReason = $"購物使用紅利點數-訂單編號[{oh.Id:D9}]",
                            RefKey = oh.Id,
                            Type = BonusLogTypeEnum.Redeem,
                            EnableIdempotencyByRefKey = true
                        });

                        if (!bonusResult.Success)
                            throw new Exception(bonusResult.Message ?? "紅利點數扣除失敗，無法建立訂單。");
                    }
                }
                await loginUserData.SaveChanges(oh);   // 這裡需要 Save 一次，拿到穩定的 oh.Id
            }
            else
            {
                // 新建訂單（正式或暫存）
                oh = mapper.Map<Order_Header>(dto);
                oh.FK_WebsiteId = websiteId;
                oh.FK_UUID = uuid;
                oh.Fk_Tid = token.RefreshToken;
                oh.Fk_UserId = userId;
                oh.CreationTime = now;

                db.Order_Headers.Add(oh);
                await db.SaveChangesAsync();           // 取得 oh.Id 給後續 detail 用

                if (!isTemp && detailResult != null)
                {
                    oh.Subtotal = detailResult.Subtotal;
                    oh.Bonus = detailResult.TotalBonus;
                    if (detailResult.TotalBonus > 0)
                    {
                        var bonusResult = await bonusManagementAppService.SaveTransaction(new CreateUserTransactionDto
                        {
                            IsSendMail = false,
                            TransactionOperation = "-",
                            MemberUUID = new List<Guid> { uuid },
                            TransactionPoint = detailResult.TotalBonus,
                            TransactionReason = $"購物使用紅利點數-訂單編號[{oh.Id:D9}]",
                            RefKey = oh.Id,
                            Type = BonusLogTypeEnum.Redeem
                        });
                        if (!bonusResult.Success) throw new Exception(bonusResult.Message ?? "紅利點數扣除失敗，無法建立訂單。");
                    }
                    var bonusSetting = await bonusManagementAppService.GetBonusSettingForEdit();
                    if (bonusSetting != null && bonusSetting.RewardRatePercent != null && bonusSetting.RewardRatePercent > 0)
                    {
                        var earnPoints = (int)Math.Floor(detailResult.Subtotal * bonusSetting.RewardRatePercent.Value / 100);
                        if (earnPoints > 0)
                        {
                            oh.GetBonus = earnPoints;
                        }
                    }
                    await db.SaveChangesAsync();
                }
            }

            return oh;
        }
        private List<Prod_Log> BuildLogSection(
            Order_Header header,
            DetailBuildResult? detailResult,
            Guid uuid)                 // 型別同樣依你實際情況調整
        {
            var logs = new List<Prod_Log>();

            if (detailResult == null) return logs;   // 暫存單不需要 log

            foreach (var sc in detailResult.ShoppingCarts)
            {
                var stock = detailResult.StockDict[sc.FK_PSid];

                logs.Add(new Prod_Log
                {
                    FK_Pid = stock.FK_Pid,
                    FK_UserId = header.CreatorUserId,
                    UUID = uuid,
                    Action = LogActionEnum.加入訂單
                });
            }

            return logs;
        }
        private async Task SaveOrderAsync(
            Order_Header header,
            DetailBuildResult? detailResult,
            List<Prod_Log> logs,
            DateTime now,
            long? userId,
            bool isTemp)
        {
            // 暫存單不用建立明細 / log
            if (isTemp || detailResult == null)
                return;

            var ods = new List<Order_Details>();

            foreach (var sc in detailResult.ShoppingCarts)
            {
                // 你原本建立 Order_Details 的欄位搬過來這裡
                ods.Add(new Order_Details
                {
                    FK_OId = header.Id,
                    FK_SCId = sc.Id,
                    CreationTime = now,
                });

                // 如果庫存變成 0，要把商品狀態改成售完
                var stock = detailResult.StockDict[sc.FK_PSid];
                var prod = stock.Prod;
                if (stock.Stock == 0 && prod != null && prod.Status != ProdStatusEnum.售完)
                {
                    prod.oStatus = prod.Status;
                    prod.Status = ProdStatusEnum.售完;
                }
            }

            db.Order_Details.AddRange(ods);
            db.Prod_Logs.AddRange(logs);

            // 這次 SaveChanges：
            // 會把 ShoppingCart / Prod_Stock / Prod / Order_Details / Prod_Log 一次寫入
            await db.SaveChangesAsync();
        }
        private async Task FillPaymentMessageAndSendMailAsync(
            OrderHeaderAddDto dto,
            long websiteId,
            Order_Header header,
            ResponseMessageDto output)
        {
            // 先查付款方式名稱
            var paymentType = await (
                from pt in db.PaymentTypes
                join ptv in db.PaymentTypesValues on pt.Id equals ptv.FK_PaymentTypesId
                join tp in db.ThirdParties on pt.FK_ThirdPartyId equals tp.Id
                where ptv.FK_WebsiteId == websiteId
                where pt.Id == header.Payment
                select tp.Title
            ).FirstOrDefaultAsync();

            var mailoutput = new ResponseMessageDto { Success = true };

            // 正式單才寄信
            if (!dto.IsTemp)
            {
                mailoutput = await SendMail(header.Id);
            }

            if (paymentType != null)
            {
                var createTime = header.CreationTime;
                switch (paymentType)
                {
                    default:
                        output.Message =
                            $"Default,{header.Id},{createTime:yyyy-MM-dd HH:mm}, {createTime.Year}年<span>{createTime.Month}月{createTime.Day + 1}日23點59分</span>";
                        break;
                    case "支付連":
                        output.Message = $"PCHomePay,{header.Id},{createTime:yyyy-MM-dd HH:mm}";
                        break;
                    case "LINE Pay":
                        output.Message = $"LinePay,{header.Id},{createTime:yyyy-MM-dd HH:mm}";
                        break;
                    case "綠界支付":
                        output.Message = $"ECPay,{header.Id},{createTime:yyyy-MM-dd HH:mm}";
                        break;
                }
            }

            if (!mailoutput.Success)
            {
                // 不 rollback 訂單，但把錯誤訊息回給前端
                output.Error = mailoutput.Message;
            }
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
                    string ship_text = "";
                    if (result.Shipping == 0)
                    {
                        ship_text = "郵寄掛號";
                    }
                    else
                    {
                        var ls = db.LogisticsSettings.Where(e => e.Id == result.Shipping).Select(e => e.LogisticsType).FirstOrDefault();
                        ship_text = ls.ToString().Replace("_", "/").Replace("Seven", "7-11");
                    }

                    OrderHeaderGetOneDto output = new OrderHeaderGetOneDto()
                    {
                        Id = result.Id,
                        Orderer = result.Orderer,
                        OrdererTelePhone = result.OrdererTelePhone == null ? "-" : result.OrdererTelePhone,
                        OrdererCellPhone = result.OrdererCellPhone,
                        OrdererEmail = result.OrdererEmail,
                        Recipient = result.Recipient,
                        RecipientTelePhone = result.RecipientTelePhone == null ? "-" : result.RecipientTelePhone,
                        RecipientCellPhone = result.RecipientCellPhone,
                        RecipientAddress = result.RecipientAddress.Replace(" ", ""),
                        RecipientEmail = result.RecipientEmail,
                        InvoiceRecipient = result.InvoiceRecipient,
                        InvoiceTitle = result.InvoiceTitle,
                        InvoiceType = result.InvoiceType,
                        InvoiceTypeTitle = result.InvoiceType.ToString(),
                        Carrier = result.Carrier,
                        UniformId = result.UniformId,
                        InvoiceAddress = result.InvoiceAddress,
                        Payment = result.Payment.ToString(),
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
                            else if (payment.Code.ToLower().StartsWith("ecpay"))
                            {
                                output.Payment = "綠界支付-" + payment.Title?.ToString() ?? "";
                            }
                            else
                            {
                                output.Payment = payment.Title?.ToString() ?? "";
                            }
                            output.ThirdParties = payment.FK_ThirdPartyId;

                            output.CanRefund = payment.CanRefund;

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
                        else order_headers = await db.Order_Headers.Where(e => ohids.Contains(e.Id) && e.Fk_Tid == checktoken.RefreshToken).ToListAsync();
                    }
                    else throw new Exception("查無Token資料");
                }
                else order_headers = await db.Order_Headers.Where(e => ohids.Contains(e.Id)).ToListAsync();

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
                                        property.SetValue(temp_output, "小姐/先生");
                                        break;
                                }
                            }
                            else if (property.Name.EndsWith("Address")) property.SetValue(temp_output, value.Replace(" ", ""));
                        }
                    }

                    var shipping = await db.LogisticsSettings.Where(e => e.FK_WebsiteId == WebsiteId && e.Id == order_header.Shipping).FirstOrDefaultAsync();
                    var shipping_str1 = shipping?.Title ?? "";
                    var shipping_str3 = (shipping?.LogisticsType ?? ShippingTypeEnum.郵寄掛號).ToString().Replace("_", "/");
                    temp_output.Shipping = shipping_str1 != "" ? shipping_str3 != "" ? $"{shipping_str1}　{shipping_str3}" : $"{shipping_str1}" : "";
                    temp_output.LogisticsType = ((int)shipping?.LogisticsType);
                    temp_output.LogisticsTypeStr = "CVS";

                    switch (shipping?.LogisticsType)
                    {
                        case ShippingTypeEnum.綠界_大宗寄倉_全家:
                        case ShippingTypeEnum.綠界_大宗寄倉_711超商:
                        case ShippingTypeEnum.綠界_大宗寄倉_711冷凍店取:
                        case ShippingTypeEnum.綠界_大宗寄倉_萊爾富:
                            temp_output.LogisticsSubTypeStr = "B2C";
                            break;
                        case ShippingTypeEnum.綠界_門市寄取_711超商:
                            temp_output.LogisticsSubTypeStr = "C2C711";
                            break;
                        case ShippingTypeEnum.綠界_門市寄取_全家:
                            temp_output.LogisticsSubTypeStr = "C2CFAMI";
                            break;
                        case ShippingTypeEnum.綠界_門市寄取_萊爾富:
                            temp_output.LogisticsSubTypeStr = "C2CHILIFE";
                            break;
                        case ShippingTypeEnum.綠界_門市寄取_OK超商:
                            temp_output.LogisticsTypeStr = "CVS";
                            temp_output.LogisticsSubTypeStr = "C2COKMART";
                            break;
                        case ShippingTypeEnum.綠界_黑貓:
                        case ShippingTypeEnum.綠界_中華郵政:
                            temp_output.LogisticsTypeStr = "HOME";
                            temp_output.LogisticsSubTypeStr = "HOME";
                            break;
                    }

                    var payment = await (from pt in db.PaymentTypes
                                         join ptv in db.PaymentTypesValues on pt.Id equals ptv.FK_PaymentTypesId
                                         where ptv.FK_WebsiteId == WebsiteId
                                         where pt.Id == order_header.Payment
                                         select pt).FirstOrDefaultAsync();

                    if (payment.Code.ToLower().StartsWith("pchome")) temp_output.Payment = "支付連-" + payment.Title?.ToString() ?? "";
                    else temp_output.Payment = payment.Title?.ToString() ?? "";

                    temp_output.CanRefund = payment.CanRefund;
                    temp_output.ThirdParties = payment.FK_ThirdPartyId;

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
                                    where pp.FK_PSId == ps.Id && pp.Id == sc.FK_PriceId
                                    from p in db.Prods
                                    where p.Id == ps.FK_Pid
                                    where sc.Quantity > 0

                                    let unitPrice = sc.Price == 0 ? (pp.Price ?? 0) : sc.Price
                                    let unitBonus = sc.Bonus == null ? (pp.Bonus ?? 0) : sc.Bonus

                                    select new OrderDetailsGetAllDto
                                    {
                                        PId = p.Id,
                                        PSId = ps.Id,
                                        Title = p.Title,
                                        S1Title = ps.FK_S1id.ToString(),
                                        S2Title = ps.FK_S2id.ToString(),
                                        Description = p.Description,
                                        Price = unitPrice,
                                        BonusPrice = unitBonus,
                                        SCPrice = sc.Price,
                                        Quantity = sc.Quantity,
                                        Subtotal = unitPrice * sc.Quantity,
                                        ImagePath = ((from f in db.FileBinds.Include(e => e.fileUpload)
                                                        .Where(e => e.Sid == p.Id && e.type == (int)FileBindTypeEnum.產品)
                                                        .Where(e => e.fileUpload != null && e.fileUpload.FK_WebsiteId == p.FK_WebsiteId && e.fileUpload.ContentType.StartsWith("image"))
                                                        .OrderBy(e => e.SerNo).ThenBy(e => e.CreationTime)
                                                      select new DirectoryReleInfoDto
                                                      {
                                                          Link = (f.fileUpload != null ? (f.fileUpload.DownloadFileName ?? "/images/noImg.jpg") : "/images/noImg.jpg").Replace("upload", $"upload/{orgName}").Replace("//", "/")
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
                            if (price != null && price != 0) item.Price = price ?? 0;
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
                                            if (temp_detail.OldPrice > 0 && temp_detail.OldPrice != temp_detail.Price)
                                            {
                                                old_price = temp_detail.OldPrice;
                                                new_price = temp_detail.Price;
                                                temp_detail.Describe = "商品規格價格更動";
                                                change = true;
                                            }
                                            if (change)
                                            {
                                                subtotal += (new_price * new_quantity) - (old_price * old_quantity);
                                                ohdata[0].Subtotal = subtotal.ToString("#,##0");
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
                            decimal subtotal = 0;
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
                            subtotal = subtotal - (ohdata.Discount ?? 0) - (ohdata.Bonus ?? 0);
                            if (subtotal == dto.Subtotal)
                            {
                                ohdata.Subtotal = (int)Math.Round(subtotal, MidpointRounding.AwayFromZero);
                                ohdata.State = OrderStatusEnum.待確認;
                                ohdata.LastModifierUserId = userid;
                                ohdata.LastModificationTime = DateTime.Now;
                                db.SaveChanges();
                                response.Success = true;
                            }
                            else
                            {
                                Console.WriteLine($"-------------錯誤訊息查看-------------");
                                Console.WriteLine($"Order=>OrderRepay：計算金額與前端傳入金額不符，計算金額：{subtotal}，前端傳入金額：{dto.Subtotal}");
                                throw new Exception("變更訂單發生錯誤");
                            }
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
                                    temp_new_order_detail.Step = old_order_detail.Step;
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
                        .Where(e => uuids.Contains(e.FK_UUID) && !e.IsTemp)
                        .OrderByDescending(e => e.CreationTime).ToListAsync();

                    response.Page_Total = (int)Math.Ceiling(order_headers.Count / 8.0);
                    order_headers = order_headers.Skip((page - 1) * 8).Take(8).ToList();

                    var paymentList = await db.PaymentTypes.ToListAsync();

                    foreach (var order_header in order_headers)
                    {
                        // 判斷訂單在Member可執行的動作 取消訂單/重新付款
                        var temp_OrderHeader = await GetHeaderOne(order_header.Id);
                        DateTime past24Hours = (DateTime.Now).AddHours(-24);
                        var isintime = false;
                        // 按照重新付款、完成訂單、創建訂單的順序判斷24小時內
                        if (order_header.RepayDate != null && order_header.RepayDate >= past24Hours) isintime = true;
                        else if (order_header.CompletedDate != null && order_header.CompletedDate >= past24Hours) isintime = true;
                        else if (order_header.CreationTime >= past24Hours) isintime = true;

                        var canRefund = paymentList.Where(p => p.Id == order_header.Payment).FirstOrDefault().CanRefund;

                        if (isintime && new List<int>() { (int)OrderStatusEnum.待確認, (int)OrderStatusEnum.待付款 }.Contains((int)order_header.State)) temp_OrderHeader.Action = "Cancel";
                        else if (isintime && (int)OrderStatusEnum.已付款 == (int)order_header.State && canRefund) temp_OrderHeader.Action = "Cancel";
                        else if (canRefund && order_header.State == OrderStatusEnum.付款失敗) temp_OrderHeader.Action = "Repay";
                        else temp_OrderHeader.Action = "";

                        var temp_OrderDetails = new List<ShoppingCartDisplayDto>();
                        var order_details = await db.Order_Details.Where(e => e.FK_OId == order_header.Id).ToListAsync();
                        foreach (var order_detail in order_details)
                        {
                            var shoppingCart = await db.ShoppingCarts.Where(e => e.Id == order_detail.FK_SCId && e.Quantity > 0 && (e.Price > 0 || e.Bonus > 0) && e.IsOrder).FirstOrDefaultAsync();
                            if (shoppingCart != null)
                            {
                                temp_OrderDetails.Add(await shoppingCartAppService.GetDropOne(shoppingCart.Id, true));
                            }
                        }
                        output.Add(new OrderDataGetDto()
                        {
                            OrderHeader = temp_OrderHeader,
                            OrderDetails = temp_OrderDetails
                        });
                    }
                }
                response.OrderData = output;
                response.Success = true;

                await CheckECPayExpiredOrders(true);
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
            long WebsiteId = await loginUserData.GetWebsiteId();

            var removeList = new List<int> { };

            var thirdPartyKeypairValues = await (from tpkv in db.ThirdPartyKeypairValues
                                                 join tpk in db.ThirdPartyKeypairs on tpkv.FK_ThirdPartyKeypairId equals tpk.Id
                                                 join tp in db.ThirdParties on tpk.FK_TPid equals tp.Id
                                                 where tp.Title == "綠界物流"
                                                 where tpkv.FK_WebsiteId == WebsiteId
                                                 select new KeyValueDto() { Key = tpk.Code, Value = tpkv.Value }).ToListAsync();

            if (!thirdPartyKeypairValues.Any()) removeList = Enumerable.Range(8, 10).ToList();
            else
            {
                var thirdPartyDict = thirdPartyKeypairValues.ToDictionary(e => e.Key, e => e.Value);
                if (thirdPartyDict.GetValueOrDefault("EnableB2C") == "false") removeList.AddRange(Enumerable.Range(8, 4).ToList());
                if (thirdPartyDict.GetValueOrDefault("EnableC2C") == "false") removeList.AddRange(Enumerable.Range(12, 4).ToList());
                //if (thirdPartyDict.GetValueOrDefault("EnableHomeDelivery") == "false") removeList.AddRange(Enumerable.Range(16, 2).ToList());
                removeList.AddRange(Enumerable.Range(16, 2).ToList());
            }

            Dictionary<string, int> shippingTypeEnums = Enum.GetValues<ShippingTypeEnum>()
                                                                                                              .Where(e => !removeList.Contains((int)e))
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
        public List<SelectDto> GetFreightStatusTypeEnum()
        {
            return EnumHelper.EnumToKeyValueList<FreightStatusTypeEnum>();
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
                                    foreach (var sc in shoppingCarts)
                                    {
                                        var prod_stock = await db.Prod_Stocks.Include(e => e.Prod).Where(e => e.Id == sc.FK_PSid).FirstOrDefaultAsync();
                                        if (prod_stock != null)
                                        {
                                            if (prod_stock.Prod != null && prod_stock.Prod.Status == ProdStatusEnum.售完)
                                            {
                                                if (prod_stock.Prod.oStatus == null) prod_stock.Prod.Status = ProdStatusEnum.一般;
                                                else prod_stock.Prod.Status = prod_stock.Prod.oStatus.Value;
                                            }
                                            prod_stock.Stock += sc.Quantity;
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
                var StoreSet = await storeSetAppService.getValues(new StoreSetGetValueInput { SiteId = WebsiteID, key = "HasInvoice" });
                var order_header = await db.Order_Headers.Where(e => e.Id == ohid).FirstOrDefaultAsync();
                var order_details = await GetOrderDetails(ohid);
                bool hasInvoice =
                    StoreSet != null && StoreSet.Success && StoreSet.detailItem != null && StoreSet.detailItem.value != null &&
                    string.Join(",", StoreSet.detailItem.value) != "DisabledInvoice";


                if (order_header != null && order_details != null)
                {
                    var InvoiceRecipient = order_header.InvoiceRecipient == 1 ? "訂購人" : "收件人";
                    var InvoiceTable = string.Empty;
                    if (hasInvoice)
                    {
                        InvoiceTable = $"""
                        <tr class='thead'>
                            <td colspan='6'>發票類型：{order_header.PersonalInvoiceType}</td>
                        </tr>
                        """;
                        if (order_header.InvoiceType == InvoiceTypeEnum.個人發票)
                        {
                            if (order_header.PersonalInvoiceType != null && order_header.PersonalInvoiceType != PersonalInvoiceTypeEnum.紙本發票)
                            {
                                InvoiceTable += $"""
                                    <tr>
                                        <td colspan='2'>{order_header.PersonalInvoiceType}</td>
                                        <td colspan='4'>{order_header.Carrier}</td>
                                    </tr>
                                """;
                            }
                        }
                        else if (order_header.InvoiceType == InvoiceTypeEnum.公司發票)
                        {
                            InvoiceTable += $"<tr>" +
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
                        InvoiceTable += $"""<tr class='thead'><td scope='col' colspan='6'>發票寄送：{InvoiceRecipient}</td></tr>""";
                    }

                    var Shipping = await db.LogisticsSettings.Where(e => e.Id == order_header.Shipping).FirstOrDefaultAsync();
                    var PaymentType = await db.PaymentTypes.Where(e => e.Id == order_header.Payment).Select(e => new { e.Title, e.FK_ThirdPartyId }).FirstOrDefaultAsync();
                    if (PaymentType == null) throw new Exception("查無付款方式資料");

                    var ThirdParty = await (from tpk in db.ThirdPartyKeypairs
                                            join tpkv in db.ThirdPartyKeypairValues on tpk.Id equals tpkv.FK_ThirdPartyKeypairId
                                            where tpk.FK_TPid == PaymentType.FK_ThirdPartyId
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
                    if (PaymentType.Title == "ATM")
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
                    else if (order_header.Payment == 29)
                    { //郵政劃撥
                        PaymentTable = $"<tr>" +
                                                        $"<td colspan='6' class='text-red'>您選擇的付款方式為郵政劃撥，目前尚未付款完成，請您於繳費期限內完成，繳費完成後請主動與公司客服聯絡。若逾期未付清款項將自動取消本訂單，謝謝。</td>" +
                                                        $"</tr>" +
                                                         $"<tr>" +
                                                         $"<td colspan='2'  scope='row' class='text-red'>繳費期限</td>" +
                                                         $"<td colspan='4'  class='text-red'>{order_header.CreationTime.AddDays(7).ToString("yyyy/MM/dd")}</td>" +
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
                        DetailsTable += $@"<tr>
                            <td  colspan='2' class='text-start'>{data.Title}</td>
                            <td>{Specification}</td>
                            <td class='text-end'>{(
                                data.Price > 0 ?
                                    data.BonusPrice != null && data.BonusPrice > 0 ?
                                        data.Price.ToString("$#,##0") + $"{data.BonusPrice.Value.ToString("Ⓟ#,##0")}" :
                                        data.Price.ToString("$#,##0") :
                                    data.BonusPrice != null && data.BonusPrice > 0 ?
                                        $"{data.BonusPrice.Value.ToString("Ⓟ#,##0")}" : 0
                            )}</td>
                            <td class='text-center'>{data.Quantity}</td>
                            <td class='text-end'>{(
                                data.Price > 0 ?
                                    data.BonusPrice != null && data.BonusPrice > 0 ?
                                        (data.Price * data.Quantity).ToString("$#,##0") + $"{(data.BonusPrice * data.Quantity).Value.ToString("Ⓟ#,##0")}" :
                                        (data.Price * data.Quantity).ToString("$#,##0") :
                                    data.BonusPrice != null && data.BonusPrice > 0 ?
                                        $"{(data.BonusPrice * data.Quantity).Value.ToString("Ⓟ#,##0")}" : 0
                            )}</td>
                        </tr>";
                    }

                    var OrdererEmailSecret = (order_header.OrdererEmail.Length > 5 ? order_header.OrdererEmail.Substring(0, 4) : order_header.OrdererEmail.Substring(0, 1)) + "**********";
                    order_header.OrdererCellPhone = (order_header.OrdererCellPhone.Length > 4 ? order_header.OrdererCellPhone.Substring(0, 4) : order_header.OrdererCellPhone.Substring(0, 1)) + "******";
                    order_header.OrdererTelePhone = !string.IsNullOrEmpty(order_header.OrdererTelePhone)
                    ? order_header.OrdererTelePhone.Length > 3
                        ? order_header.OrdererTelePhone.Substring(0, 3) + "******"
                        : order_header.OrdererTelePhone.Substring(0, 1) + "******"
                    : "";
                    var OrdererSex = order_header.OrdererSex == 1 ? "先生" : order_header.OrdererSex == 2 ? "小姐" : "小姐/先生";
                    order_header.RecipientAddress = order_header.RecipientAddress.Replace(" ", "").Substring(0, 6) + "**********";
                    order_header.RecipientCellPhone = (order_header.RecipientCellPhone.Length > 4 ? order_header.RecipientCellPhone.Substring(0, 4) : order_header.RecipientCellPhone.Substring(0, 1)) + "******";
                    order_header.RecipientTelePhone = !string.IsNullOrEmpty(order_header.RecipientTelePhone)
                     ? order_header.RecipientTelePhone.Length > 3
                         ? order_header.RecipientTelePhone.Substring(0, 3) + "******"
                         : order_header.RecipientTelePhone.Substring(0, 1) + "******"
                     : "";
                    var RecipientSex = order_header.RecipientSex == 1 ? "先生" : order_header.RecipientSex == 2 ? "小姐" : "小姐/先生";

                    var mailhtml = @$"<div class='text-size1'><h2 class='text-red'>親愛的會員，您好！</h2>
                 <br/>
                 <div>非常感謝您的訂購，以下為您的訂購清單，而非付款收據，為保障您的安全，部分訊息將以'*'標記。</div>
                 <ul>
                 <li>若您使用ATM轉帳付款，請於訂購日起兩日內轉帳，繳費完成後請主動與公司客服聯絡。</li>
                 <li>若您使用其他付款方式或貨到付款，您可以至訂單查詢了解訂單詳情與處理進度。</li>
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
                {(
                    order_header.Bonus != null && order_header.Bonus > 0 ?
                    $@"<tr>
                        <td colspan='6' class='text-end text-bold'>總使用紅利<span class='text-red ms-1 text-size1_25'>{order_header.Bonus.Value.ToString("$#,##0")}</span></td>
                    </tr>" :
                    ""
                 )}
                 <tr class='thead'><td colspan='6' scope='col'>運送方式：<span class='text-red ms-1 text-size1_5'>{Shipping!.Title}　{Shipping.LogisticsType}</span></td></tr>
                 <tr class='thead'><td colspan='6'  scope='col'>付款方式：<span class='text-red ms-1 text-size1_5'>{PaymentType.Title}</span></td></tr>
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
                    if (!string.IsNullOrEmpty(Website.ContactMail)) cc.Email = Website.ContactMail;
                    if (!string.IsNullOrEmpty(Website.Contact)) cc.Name = Website.Contact ?? cc.Name;
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
                if (webSiteId == 0) webSiteId = loginUserData.GetFrontWebsiteId();
                var order = await db.Order_Headers.Where(e => e.Id == dto.Id && e.FK_WebsiteId == webSiteId).FirstOrDefaultAsync();
                if (order == null) throw new Exception("訂單不存在");

                var oldStatus = order.State;
                var newStatus = dto.Status;

                // 若狀態相同則只更新備註
                if (oldStatus == newStatus && dto.Memo != null)
                {
                    order.Memo = dto.Memo;
                    await loginUserData.SaveChanges(order);
                    response.Success = true;
                    return response;
                }

                var strategy = db.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await db.Database.BeginTransactionAsync();
                    // 3-1) 更新狀態（沿用你既有邏輯，含補庫存/CompletedDate/Cancel mail...等）
                    response = await OrderStateChange(order.Id, (int)newStatus);
                    if (!response.Success) throw new Exception(response.Message ?? "訂單狀態更新失敗");

                    // 3-2) 重新抓一次 order（避免 OrderStateChange 內部 SaveChanges 造成你這裡 entity 狀態不一致）
                    order = await db.Order_Headers
                        .Where(e => e.Id == dto.Id && e.FK_WebsiteId == webSiteId)
                        .FirstOrDefaultAsync();

                    if (order == null) throw new Exception("訂單不存在");

                    // 3-3) 更新 Memo
                    if (dto.Memo != null)
                    {
                        order.Memo = dto.Memo;
                    }

                    await loginUserData.SaveChanges(order);

                    // 3-4) 狀態事件：已完成 -> 發回饋
                    if (newStatus == OrderStatusEnum.已完成)
                    {
                        var earnPoints = order.GetBonus ?? 0;
                        if (earnPoints > 0)
                        {
                            var earnReason = $"完成訂單回饋紅利-訂單編號[{order.Id:D9}]";

                            // 你需要在 Bonus service 補這支：EnsureEarnByOrderAsync
                            var earnResult = await bonusManagementAppService.SaveTransaction(new CreateUserTransactionDto
                            {
                                MemberUUID = new List<Guid> { order.FK_UUID },
                                RefKey = order.Id,
                                TransactionOperation = "+",
                                TransactionPoint = earnPoints,
                                IsSendMail = false,
                                TransactionReason = earnReason,
                                Type = BonusLogTypeEnum.Earn,
                                EnableIdempotencyByRefKey = true,
                            });

                            if (!earnResult.Success)
                                throw new Exception(earnResult.Message ?? "回饋紅利發送失敗");
                        }
                    }

                    // 3-5) 狀態事件：已取消/付款失敗 -> 補還折抵 + 追回回饋（若已發）
                    if (newStatus == OrderStatusEnum.已取消 || newStatus == OrderStatusEnum.付款失敗)
                    {
                        // (A) 補還折抵
                        var redeemed = order.Bonus ?? 0;
                        if (redeemed > 0)
                        {
                            var refundReason = $"取消/作廢訂單退回折抵紅利-訂單編號[{order.Id:D9}]";

                            // 你需要在 Bonus service 補這支：RefundRedeemByOrderAsync
                            var refundResult = await bonusManagementAppService.RefundRedeemByOrderAsync(
                                order.FK_UUID,
                                order.Id,
                                refundReason
                            );

                            if (!refundResult.Success)
                                throw new Exception(refundResult.Message ?? "折抵紅利退回失敗");
                        }

                        // (B) 追回已發回饋（若曾發）
                        var earnPoints = order.GetBonus ?? 0;
                        if (earnPoints > 0)
                        {
                            var revokeReason = $"取消/作廢訂單追回回饋紅利-訂單編號[{order.Id:D9}]";

                            // 你需要在 Bonus service 補這支：RevokeEarnByOrderAsync
                            var revokeResult = await bonusManagementAppService.RevokeEarnByOrderAsync(
                                order.FK_UUID,
                                order.Id,
                                revokeReason
                            );

                            if (!revokeResult.Success)
                                throw new Exception(revokeResult.Message ?? "回饋紅利追回失敗");
                        }
                    }

                    await tx.CommitAsync();
                });

                response.Success = true;
                await loginUserData.SetLogs(JsonConvert.SerializeObject(dto), JsonConvert.SerializeObject(response));
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
                                where oh.FK_UUID == UUID && !oh.IsTemp
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
                    if (PaymentType.Id == 1) Remind = "如有貨款需退回，請您與原訂購商店/網站聯繫。";
                    else if (order_header.refundTransactionId != null)
                    {
                        RefundTransactionId = $@"<tr>
                                                                        <td scope='row' class='text-bold' style='text-align: center; background-color: #F2F2F2;'>{ThirdParty?.Title.Replace(" ", "") ?? ""}退款序號</td>
                                                                        <td>{order_header.refundTransactionId}</td>
                                                                    </tr>";
                        AmountTitle = "退款金額";
                        Remind = "若欲詢問退貨退款相關問題，請您與原訂購商店/網站聯繫。";
                        var RefundText = PaymentType.RefundWorkDay < 0 ? "如有貨款需退回，請您與原訂購商店/網站聯繫。" : PaymentType.RefundWorkDay == 0 ? $"貨款將即時退回，{Remind}" : $"貨款將在{PaymentType.RefundWorkDay}個工作天內退回，{Remind}";
                        Remind = $"<div class='text-bold text-red'>貼心提醒：{RefundText}</div>";
                        MailTitle = "退款通知";
                    }

                    var DetailsTable = @"<table>
                                                                <tbody>
                                                                <tr class='text-bold' style='text-align: center; background-color: #F2F2F2;'>
                                                                     <td scope='col' colspan='2' class='text-center'>購物明細</td>
                                                                     <td scope='col' class='text-center'>規格</td>
                                                                     <td scope='col' class='text-center'>單價</td>
                                                                     <td scope='col' class='text-center'>數量</td>
                                                                     <td scope='col' class='text-center'>小計</td>
                                                                 </tr>";
                    var order_details = await GetOrderDetails(ohid);
                    foreach (var data in order_details)
                    {
                        var Specification = data.S1Title != "" ? data.S2Title != "" ? $"{data.S1Title}、{data.S2Title}" : data.S1Title : "";
                        DetailsTable += $"<tr>" +
                                                        $"<td  colspan='2'>{data.Title}</td>" +
                                                        $"<td style='text-align: center;'>{Specification}</td>" +
                                                        $"<td style='text-align: right;'>{(
                                                            data.Price > 0 ?
                                                                data.BonusPrice != null && data.BonusPrice > 0 ?
                                                                    data.Price.ToString("$#,##0") + $"{data.BonusPrice.Value.ToString("Ⓟ#,##0")}" :
                                                                    data.Price.ToString("$#,##0") :
                                                                data.BonusPrice != null && data.BonusPrice > 0 ?
                                                                    $"{data.BonusPrice.Value.ToString("Ⓟ#,##0")}" : 0
                                                        )}</td>" +
                                                        $"<td style='text-align: center;'>{data.Quantity}</td>" +
                                                        $"<td style='text-align: right;'>{(
                                                            data.Price > 0 ?
                                                                data.BonusPrice != null && data.BonusPrice > 0 ?
                                                                    (data.Price * data.Quantity).ToString("$#,##0") + $"{(data.BonusPrice * data.Quantity).Value.ToString("Ⓟ#,##0")}" :
                                                                    (data.Price * data.Quantity).ToString("$#,##0") :
                                                                data.BonusPrice != null && data.BonusPrice > 0 ?
                                                                    $"{(data.BonusPrice * data.Quantity).Value.ToString("Ⓟ#,##0")}" : 0
                                                        )}</td>" +
                                                        $"</tr>";
                    }
                    DetailsTable += "</tbody></table>";

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
                                                            {DetailsTable}
                                                            <br/>
                                                             {Remind}
                                                            <br/>
                                                             <hr/>
                                                             <div class='text-bold text-red'>提醒您：此封『{MailTitle}』為系統發出，請勿直接回覆。</div>
                                                             <div class='text-bold text-red'>客服人員均不會要求消費者更改帳號或要求以ATM重新轉帳匯款。</div>
                                                             <div class='text-bold text-red'>若有上述情形，請立即撥打165防詐騙專線查詢。</div>
                                                             <hr/>
                                                        </div>";
                    var mailcss = "*{ font-family: sans-serif; } .text-bold {  font-weight: bold; } .text-red {  color: red; } table { border-collapse: collapse; border: 2px solid #8c8c8c; letter-spacing: 1px; width: 600px; margin: 1rem 0 1rem 0; } th,td { border: 1px solid #a0a0a0; padding: 8px 10px; }";

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
        public async Task<ResponseMessageDto> CheckECPayExpiredOrders(bool isuser)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            ECPayThirdPartyDataDto ThirdPartyData = new ECPayThirdPartyDataDto();

            try
            {
                DateTime DateTimeNow = DateTime.Now;
                var WebsiteID = configuration.GetValue<long>("WebConfig:SiteId");
                if (WebsiteID == 0) WebsiteID = await loginUserData.GetWebsiteId();
                Guid UUID = await tokenAppService.GetUUID();

                var thirdPartyKeypairValues = await (from tpkv in db.ThirdPartyKeypairValues
                                                     join tpk in db.ThirdPartyKeypairs on tpkv.FK_ThirdPartyKeypairId equals tpk.Id
                                                     join tp in db.ThirdParties on tpk.FK_TPid equals tp.Id
                                                     where tp.Title == "綠界支付"
                                                     where tpkv.FK_WebsiteId == WebsiteID
                                                     select new KeyValueDto() { Key = tpk.Code, Value = tpkv.Value }).ToListAsync();

                if (!thirdPartyKeypairValues.Any()) throw new Exception("查無ThirdParty資料");

                var thirdPartyDict = thirdPartyKeypairValues.ToDictionary(e => e.Key, e => e.Value);
                ThirdPartyData.MerchantID = thirdPartyDict.GetValueOrDefault("MerchantID") ?? throw new Exception("商家未確實設置綠界支付資料");
                ThirdPartyData.PlatformID = thirdPartyDict.GetValueOrDefault("PlatformID") ?? "";
                ThirdPartyData.HashKey = thirdPartyDict.GetValueOrDefault("HashKey") ?? throw new Exception("商家未確實設置綠界支付資料");
                ThirdPartyData.HashIV = thirdPartyDict.GetValueOrDefault("HashIV") ?? throw new Exception("商家未確實設置綠界支付資料");

                ThirdPartyData.ExpireDate = thirdPartyDict.GetValueOrDefault("ExpireDate") ?? "";
                double expireDate = GetExpireDate(ThirdPartyData.ExpireDate, 3, 1, 60);

                ThirdPartyData.StoreExpireDate_CVS = thirdPartyDict.GetValueOrDefault("StoreExpireDate_CVS") ?? "";
                double storeExpireCVS = GetExpireDate(ThirdPartyData.StoreExpireDate_CVS, 7, 1, 30);

                ThirdPartyData.StoreExpireDate_Barcode = thirdPartyDict.GetValueOrDefault("StoreExpireDate_Barcode") ?? "";
                double storeExpireBarcode = GetExpireDate(ThirdPartyData.StoreExpireDate_Barcode, 7, 1, 30);

                var payments = new List<long>([21, 22, 23]);
                var query = db.Order_Headers.Where(e => e.FK_WebsiteId == WebsiteID && payments.Contains(e.Payment) && e.State == OrderStatusEnum.待付款 && !e.IsTemp);
                if (isuser) query = query.Where(e => e.FK_UUID == UUID);
                List<Order_Header> order_Headers = await query.ToListAsync();
                var hasChange = false;

                foreach (var order in order_Headers)
                {
                    switch (order.Payment)
                    {
                        case 21:
                            if (DateTimeNow > order.CreationTime.AddDays(expireDate).AddHours(1))
                            {
                                order.State = OrderStatusEnum.付款失敗;
                                hasChange = true;
                            }
                            break;
                        case 22:
                            if (DateTimeNow > order.CreationTime.AddDays(storeExpireBarcode + 2).AddHours(1))
                            {
                                order.State = OrderStatusEnum.付款失敗;
                                hasChange = true;
                            }
                            break;
                        case 23:
                            if (DateTimeNow > order.CreationTime.AddDays(storeExpireCVS).AddHours(1))
                            {
                                order.State = OrderStatusEnum.付款失敗;
                                hasChange = true;
                            }
                            break;
                    }
                }
                if (hasChange) db.SaveChanges();
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        private int GetExpireDate(string value, int defaultValue, int min, int max)
        {
            if (string.IsNullOrEmpty(value)) return defaultValue;
            if (!int.TryParse(value, out var parsed)) return defaultValue;
            return Math.Clamp(parsed, min, max);
        }
        public async Task<ResponseMessageDto> GetForPaymentAsync(long ohid)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetCommonWebsiteId();
                List<OrderStatusEnum> canPayStates = new List<OrderStatusEnum>() { OrderStatusEnum.待確認, OrderStatusEnum.待付款, OrderStatusEnum.付款失敗 };
                var order = await db.Order_Headers.AsNoTracking().Where(o => o.Id == ohid && canPayStates.Contains(o.State) && o.FK_WebsiteId == websiteId)
                    .Select(x => new
                    {
                        x.Id,
                        OrderNo = x.Id.ToString("D9"),
                        PaymentTime = DateTime.Now,
                        ShippingFee = x.Freight,
                        UseBonusAmount = x.Bonus ?? 0,
                        CouponDiscount = 0m,
                        Discount = x.Discount ?? 0
                    }).FirstOrDefaultAsync();
                if (order == null) throw new Exception("查無訂單資料或訂單狀態不正確");

                var details = await db.Order_Details
                    .AsNoTracking()
                    .Where(x => x.FK_OId == ohid && !x.IsDeleted)
                    .Select(x => new
                    {
                        DetailId = x.Id,
                        CartId = x.ShoppingCart != null ? x.ShoppingCart.Id : 0,
                        ProductId = x.ShoppingCart != null && x.ShoppingCart.Prod_Stock != null
                            ? x.ShoppingCart.Prod_Stock.FK_Pid
                            : 0,
                        ProductName =
                            x.ShoppingCart != null && !string.IsNullOrWhiteSpace(x.ShoppingCart.ProdName)
                                ? x.ShoppingCart.ProdName
                                : (x.ShoppingCart != null && x.ShoppingCart.Prod_Stock != null && x.ShoppingCart.Prod_Stock.Prod != null ? x.ShoppingCart.Prod_Stock.Prod.Title : null),
                        Quantity = x.ShoppingCart != null ? x.ShoppingCart.Quantity : 0,
                        UnitPrice = x.ShoppingCart != null ? x.ShoppingCart.Price : 0m
                    })
                    .ToListAsync();
                if (!details.Any()) throw new Exception("查無訂單明細資料");

                var payData = new PayOrderData
                {
                    OrderId = order.OrderNo,
                    PaymentTime = order.PaymentTime,
                    Currency = "TWD"
                };
                var prodIds = details.Select(x => x.ProductId).Where(x => x > 0).Distinct().ToList();
                var prodImages = await fileUploadAppService.getImgsFiles(new FileGetImgsInputDto { Sid = prodIds, Size = 3, Type = (int)FileBindTypeEnum.產品 });
                var Website = await db.Websites.Where(e => e.Id == websiteId).FirstOrDefaultAsync();
                foreach (var detail in details)
                {
                    if (detail.Quantity <= 0) throw new Exception($"明細 {detail.DetailId} 數量不可小於等於 0");
                    if (detail.UnitPrice < 0) throw new Exception($"明細 {detail.DetailId} 單價不可小於 0");

                    var productName = string.IsNullOrWhiteSpace(detail.ProductName)
                        ? $"商品#{detail.CartId}"
                        : detail.ProductName;

                    decimal originalLineAmount = detail.UnitPrice * detail.Quantity;
                    var imgUrl = prodImages.FirstOrDefault(x => x.Sid == detail.ProductId)?.Link ?? "";
                    if (string.IsNullOrEmpty(imgUrl) || Website == null) imgUrl = null;
                    else imgUrl = $"{Website.DefaultUrl}{imgUrl}";
                    payData.Items.Add(new PayOrderItem
                    {
                        ItemId = detail.ProductId.ToString(),
                        Name = productName,
                        Quantity = detail.Quantity,
                        OriginalUnitPrice = detail.UnitPrice,
                        OriginalLineAmount = originalLineAmount,
                        PayUnitPrice = (int)Math.Round(detail.UnitPrice, MidpointRounding.AwayFromZero),
                        PayLineAmount = (int)Math.Round(originalLineAmount, MidpointRounding.AwayFromZero),
                        ImageUrl = imgUrl,
                        IsShipping = false
                    });
                }

                decimal shippingFee = order.ShippingFee;
                if (shippingFee > 0)
                {
                    payData.Items.Add(new PayOrderItem
                    {
                        ItemId = "shipping",
                        Name = "運費",
                        Quantity = 1,
                        OriginalUnitPrice = shippingFee,
                        OriginalLineAmount = shippingFee,
                        PayUnitPrice = (int)Math.Round(shippingFee, MidpointRounding.AwayFromZero),
                        PayLineAmount = (int)Math.Round(shippingFee, MidpointRounding.AwayFromZero),
                        IsShipping = true
                    });
                }

                int totalAdjustment = 0;
                if (order.UseBonusAmount > 0)
                {
                    int bonus = order.UseBonusAmount;
                    payData.Adjustments.Add(new PayOrderAdjustment
                    {
                        Type = "Bonus",
                        Name = "紅利折抵",
                        Amount = bonus
                    });
                    totalAdjustment += bonus;
                }

                if (order.CouponDiscount > 0)
                {
                    int coupon = (int)Math.Round(order.CouponDiscount, MidpointRounding.AwayFromZero);
                    payData.Adjustments.Add(new PayOrderAdjustment
                    {
                        Type = "Coupon",
                        Name = "優惠券折抵",
                        Amount = coupon
                    });
                    totalAdjustment += coupon;
                }

                if (order.Discount > 0)
                {
                    int discount = (int)Math.Round(order.Discount, MidpointRounding.AwayFromZero);
                    payData.Adjustments.Add(new PayOrderAdjustment
                    {
                        Type = "Discount",
                        Name = "訂單折抵",
                        Amount = discount
                    });
                    totalAdjustment += discount;
                }

                var targetItems = payData.Items.Where(x => !x.IsShipping && x.PayLineAmount > 0).ToList();
                int goodsTotal = targetItems.Sum(x => x.PayLineAmount);

                if (totalAdjustment > goodsTotal) throw new Exception("折抵金額不可大於商品總額");

                if (totalAdjustment > 0)
                {
                    int allocated = 0;

                    for (int i = 0; i < targetItems.Count; i++)
                    {
                        var item = targetItems[i];
                        int share = i == targetItems.Count - 1
                            ? totalAdjustment - allocated
                            : (int)Math.Floor((decimal)totalAdjustment * item.PayLineAmount / goodsTotal);

                        if (i != targetItems.Count - 1)
                        {
                            allocated += share;
                        }

                        item.PayLineAmount -= share;
                        if (item.PayLineAmount < 0)
                        {
                            item.PayLineAmount = 0;
                        }
                    }
                }

                payData.Items = NormalizePayItems(payData.Items);
                payData.PayableAmount = payData.Items.Sum(x => x.PayLineAmount);
                response.Object = payData;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        private List<PayOrderItem> NormalizePayItems(List<PayOrderItem> items)
        {
            List<PayOrderItem> result = new List<PayOrderItem>();

            foreach (var item in items)
            {
                if (item.Quantity <= 0)
                    throw new Exception($"品項 {item.Name} 的數量不可小於等於 0");

                if (item.PayLineAmount < 0)
                    throw new Exception($"品項 {item.Name} 的支付金額不可小於 0");

                // 運費或單件商品，直接處理
                if (item.Quantity == 1)
                {
                    item.PayUnitPrice = item.PayLineAmount;
                    result.Add(item);
                    continue;
                }

                int basePrice = item.PayLineAmount / item.Quantity;
                int remainder = item.PayLineAmount % item.Quantity;

                // 可整除，維持一筆
                if (remainder == 0)
                {
                    item.PayUnitPrice = basePrice;
                    result.Add(item);
                    continue;
                }

                // 不可整除，拆成兩筆
                int lowQty = item.Quantity - remainder;
                int highQty = remainder;

                if (lowQty > 0)
                {
                    result.Add(new PayOrderItem
                    {
                        ItemId = item.ItemId,
                        Name = item.Name,
                        Quantity = lowQty,
                        OriginalUnitPrice = item.OriginalUnitPrice,
                        OriginalLineAmount = item.OriginalUnitPrice * lowQty,
                        PayUnitPrice = basePrice,
                        PayLineAmount = basePrice * lowQty,
                        IsShipping = item.IsShipping,
                        ImageUrl = item.ImageUrl
                    });
                }

                if (highQty > 0)
                {
                    result.Add(new PayOrderItem
                    {
                        ItemId = item.ItemId,
                        Name = item.Name,
                        Quantity = highQty,
                        OriginalUnitPrice = item.OriginalUnitPrice,
                        OriginalLineAmount = item.OriginalUnitPrice * highQty,
                        PayUnitPrice = basePrice + 1,
                        PayLineAmount = (basePrice + 1) * highQty,
                        IsShipping = item.IsShipping,
                        ImageUrl = item.ImageUrl
                    });
                }
            }

            return result;
        }
    }
}

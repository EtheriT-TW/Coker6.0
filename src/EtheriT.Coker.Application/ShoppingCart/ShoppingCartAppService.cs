using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Product;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace EtheriT.Coker.Application.ShoppingCart
{
    public class ShoppingCartAppService : IShoppingCartAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ITokenAppService tokenAppService;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        private readonly IProductAppService productAppService;
        private readonly IBonusManagementAppService bonusManagementAppService;
        public ShoppingCartAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITokenAppService tokenAppService,
            IConfiguration configuration,
            IMapper mapper,
            IProductAppService productAppService,
            IBonusManagementAppService bonusManagementAppService
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.tokenAppService = tokenAppService;
            this.configuration = configuration;
            this.mapper = mapper;
            this.productAppService = productAppService;
            this.bonusManagementAppService = bonusManagementAppService;
        }
        public async Task<ResponseMessageDto> UpdateUUID(Guid UserUUID, Guid TempUUID)
        {
            var response = new ResponseMessageDto();
            try
            {
                var userid = await db.FrontUsers.Where(e => e.UUID == UserUUID).Select(e => e.FK_User).FirstOrDefaultAsync();
                var oldsc = await db.ShoppingCarts.Where(e => e.UUID == TempUUID && e.IsOrder).ToListAsync();
                var tempsc = await db.ShoppingCarts.Where(e => e.UUID == TempUUID && !e.IsOrder).ToListAsync();
                var usersc = await db.ShoppingCarts.Where(e => e.UUID == UserUUID && !e.IsOrder).ToListAsync();
                var timenow = DateTime.Now;
                if (oldsc.Any())
                {
                    for (var i = 0; i < oldsc.Count; i++)
                    {
                        oldsc[i].UUID = UserUUID;
                        oldsc[i].FK_Uid = userid ?? 0;
                        oldsc[i].LastModifierUserId = userid ?? 0;
                        oldsc[i].LastModificationTime = timenow;
                    }
                }
                if (tempsc.Any())
                {
                    for (var i = 0; i < tempsc.Count; i++)
                    {
                        tempsc[i].UUID = UserUUID;
                        tempsc[i].FK_Uid = userid ?? 0;
                        tempsc[i].LastModifierUserId = userid ?? 0;
                        tempsc[i].LastModificationTime = timenow;
                    }
                    if (usersc.Any())
                    {
                        for (var i = 0; i < tempsc.Count; i++)
                        {
                            if (usersc.Find(e => e.FK_PSid == tempsc[i].FK_PSid) != null)
                            {
                                usersc[usersc.FindIndex(e => e.FK_PSid == tempsc[i].FK_PSid)].Quantity += tempsc[i].Quantity;
                                usersc[i].LastModifierUserId = userid ?? 0;
                                usersc[i].LastModificationTime = timenow;
                                tempsc[i].IsDeleted = true;
                                tempsc[i].DeletionTime = timenow;
                            }
                        }
                    }
                    db.SaveChanges();
                }
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
        private async Task<CartBonusCheckResult> CheckCartBonusEnough(Guid uuid, int incrementBonus)
        {
            var result = new CartBonusCheckResult();

            if (incrementBonus < 0)
                incrementBonus = 0;

            var bonusData = await bonusManagementAppService
                .GetQueryFrontUsersTotalAvaliableBonus(new List<Guid> { uuid });

            result.AvailableBonus = bonusData?.FirstOrDefault()?.TotalAvaliableBonus ?? 0;

            var carts = await db.ShoppingCarts
                .Where(e => e.UUID == uuid && !e.IsOrder)
                .ToListAsync();

            result.CurrentCartBonus = carts.Sum(e => (e.Bonus ?? 0) * e.Quantity);
            result.IncrementBonus = incrementBonus;
            result.TotalNeededBonus = result.CurrentCartBonus + result.IncrementBonus;
            result.IsEnough = result.AvailableBonus >= result.TotalNeededBonus;

            return result;
        }
        public async Task<ResponseMessageDto> AddUp(ShoppingCartAddUpDto dto)
        {
            ResponseMessageDto response = new ResponseMessageDto() { Success = false };

            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                var token = await tokenAppService.CheckToken(null);
                if (token.RefreshToken == null)
                    token = await tokenAppService.CreateToken();

                if (UUID == Guid.Empty)
                {
                    UUID = await db.Tokens
                        .Where(t => t.id == token.RefreshToken)
                        .Select(t => t.UUID)
                        .FirstOrDefaultAsync();

                    if (UUID == Guid.Empty)
                        throw new Exception("取得Token發生錯誤");
                }

                var userid = await db.FrontUsers
                    .Where(e => e.UUID == UUID)
                    .Select(e => e.FK_User)
                    .FirstOrDefaultAsync() ?? 0;

                var proStock = await db.Prod_Stocks.FirstOrDefaultAsync(e =>
                    dto.FK_PSid != null
                        ? e.Id == dto.FK_PSid
                        : (e.FK_Pid == dto.FK_Pid && e.FK_S1id == dto.FK_S1id && e.FK_S2id == dto.FK_S2id));

                if (proStock == null)
                    throw new Exception("查無商品規格");

                var currentStock = proStock.Stock ?? 0;
                if (currentStock <= 0)
                    throw new Exception("目前無庫存");

                Core.Models.ShoppingCart? sc = null;
                if (dto.Id != null)
                {
                    sc = await db.ShoppingCarts.FirstOrDefaultAsync(e => e.Id == dto.Id);
                    if (sc == null) throw new Exception("查無購物車資料");
                }
                else
                {
                    sc = await db.ShoppingCarts.FirstOrDefaultAsync(e =>
                        e.UUID == UUID &&
                        e.FK_PSid == proStock.Id &&
                        e.FK_PriceId == dto.FK_PriceId &&
                        !e.IsOrder);
                }

                int wantQty = Math.Max(1, dto.Quantity);

                decimal unitPrice = 0;
                int bonus = 0;
                Prod_Price? prodPrice = null;

                if (dto.FK_PriceId != null)
                {
                    prodPrice = await db.Prod_Prices
                        .FirstOrDefaultAsync(e => e.Id == dto.FK_PriceId);

                    if (prodPrice != null)
                    {
                        unitPrice = prodPrice.Price ?? 0;
                        bonus = prodPrice.Bonus ?? 0;
                    }
                }

                // ===== 紅利檢查：有使用紅利時，必須登入且整個購物車紅利足夠 =====
                var bonusSetting = await bonusManagementAppService.GetBonusSettingForEdit();
                var bonusEnabled = bonusSetting?.BonusEnabled == true;

                if (bonusEnabled && bonus > 0)
                {
                    if (userid == 0)
                        throw new Exception("請登入會員");

                    var incrementBonus = bonus * wantQty;
                    var bonusCheck = await CheckCartBonusEnough(UUID, incrementBonus);

                    if (!bonusCheck.IsEnough)
                        throw new Exception(bonusCheck.Message);
                }
                // ===== 紅利檢查結束 =====

                if (sc == null)
                {
                    if (wantQty > currentStock)
                        throw new Exception($"可購買上限 {currentStock} 件，無法再加入 {wantQty} 件");

                    var date = DateTime.Now;

                    sc = new Core.Models.ShoppingCart
                    {
                        FK_PSid = proStock.Id,
                        FK_PriceId = dto.FK_PriceId,
                        Price = unitPrice,
                        Bonus = bonus,
                        Quantity = wantQty,
                        FK_Tid = (Guid)token.RefreshToken!,
                        FK_Uid = userid,
                        UUID = UUID,
                        Ser_No = 500,
                        ProdName = dto.ProdName,
                        CreatorUserId = userid,
                        CreationTime = date
                    };

                    db.ShoppingCarts.Add(sc);
                    LogCartEventAsync(proStock.FK_Pid, userid, UUID, LogActionEnum.加入購物車, 0, wantQty);
                    db.SaveChanges();
                    response.Message = "N" + sc.Id.ToString();
                }
                else
                {
                    int newTotal = sc.Quantity + wantQty;
                    int oQuantity = sc.Quantity;

                    if (newTotal > currentStock)
                        throw new Exception($"可購買上限 {currentStock} 件（購物車已有 {sc.Quantity} 件），無法再加入 {wantQty} 件");

                    sc.Quantity = newTotal;
                    sc.Price = unitPrice;
                    sc.Bonus = bonus;
                    sc.FK_PriceId = dto.FK_PriceId;
                    sc.LastModificationTime = DateTime.Now;
                    sc.LastModifierUserId = userid;

                    LogCartEventAsync(proStock.FK_Pid, userid, UUID, LogActionEnum.加入購物車, oQuantity, newTotal);

                    await db.SaveChangesAsync();
                    response.Message = "U" + sc.Id.ToString();
                }

                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = "Error";
                response.Message = ex.Message;
            }

            return response;
        }
        public async Task<bool> checkBonusCanUse(Guid uuid, List<OrderDetailAddDto> OrderDetails)
        {
            var bonusSetting = await bonusManagementAppService.GetBonusSettingForEdit();
            var bonusEnabled = bonusSetting?.BonusEnabled == true;

            if (!bonusEnabled) return true;

            var bonusData = await bonusManagementAppService.GetQueryFrontUsersTotalAvaliableBonus(new List<Guid> { uuid });

            var ids = OrderDetails.Select(e => e.Id).ToList();

            var shoppingCarts = await db.ShoppingCarts
                .Include(e => e.Prod_Price)
                .Where(e => e.UUID == uuid && !e.IsOrder && ids.Contains(e.Id))
                .ToListAsync();

            var bonusNeeded = shoppingCarts.Sum(e =>
                e.Prod_Price == null
                    ? (e.Bonus ?? 0) * e.Quantity
                    : (e.Prod_Price.Bonus ?? 0) * e.Quantity
            );

            if (bonusNeeded == 0) return true;

            var bonus = bonusData?.FirstOrDefault();

            return bonus != null && bonus.TotalAvaliableBonus >= bonusNeeded;
        }
        public async Task<ResponseMessageDto> QuantityUpdate(List<ShoppingQuantityUpdateDto> dtos)
        {
            var response = new ResponseMessageDto
            {
                Success = true
            };

            var batchResult = new QuantityUpdateBatchResult();
            response.Object = batchResult;

            try
            {
                if (dtos == null || dtos.Count == 0)
                {
                    response.Success = true;
                    response.Message = "沒有需要更新的項目。";
                    return response;
                }

                Guid uuid = await tokenAppService.GetUUID();

                var bonusSetting = await bonusManagementAppService.GetBonusSettingForEdit();
                var bonusEnabled = bonusSetting?.BonusEnabled == true;

                var cartIds = dtos.Select(d => d.Id).Distinct().ToList();

                var carts = await db.ShoppingCarts
                    .Where(e => cartIds.Contains(e.Id) && !e.IsOrder)
                    .ToListAsync();

                var stockIds = carts.Select(c => c.FK_PSid).Distinct().ToList();
                var stocks = await db.Prod_Stocks
                    .Where(s => stockIds.Contains(s.Id))
                    .ToListAsync();

                foreach (var dto in dtos)
                {
                    var itemResult = new QuantityUpdateItemResult
                    {
                        CartId = dto.Id
                    };
                    batchResult.Items.Add(itemResult);

                    var sc = carts.FirstOrDefault(e => e.Id == dto.Id);
                    if (sc == null)
                    {
                        itemResult.Success = false;
                        itemResult.Error = "CartNotFound";
                        itemResult.Message = "查無購物車資料";
                        itemResult.OldQuantity = dto.Quantity;
                        itemResult.NewQuantity = dto.Quantity;
                        response.Success = false;
                        continue;
                    }

                    var pro_stock = stocks.FirstOrDefault(s => s.Id == sc.FK_PSid);
                    if (pro_stock == null)
                    {
                        itemResult.Success = false;
                        itemResult.Error = "StockNotFound";
                        itemResult.Message = "查無商品庫存";
                        itemResult.OldQuantity = sc.Quantity;
                        itemResult.NewQuantity = sc.Quantity;
                        response.Success = false;
                        continue;
                    }

                    var stock = pro_stock.Stock ?? 0;
                    var requested = dto.Quantity;
                    var original = sc.Quantity;

                    itemResult.OldQuantity = original;
                    itemResult.NewQuantity = original;
                    itemResult.Removed = false;

                    if (stock <= 0)
                    {
                        itemResult.Success = false;
                        itemResult.Error = "StockNotEnough";
                        itemResult.Message = "此商品目前已無庫存，請調整或移除該品項。";
                        response.Success = false;
                        continue;
                    }

                    if (requested < 0)
                    {
                        itemResult.Success = false;
                        itemResult.Error = "InvalidQuantity";
                        itemResult.Message = "數量不可小於 0。";
                        response.Success = false;
                        continue;
                    }

                    if (!bonusEnabled && (sc.Bonus ?? 0) > 0)
                    {
                        itemResult.Success = false;
                        itemResult.Error = "BonusDisabled";
                        itemResult.Message = "目前未開放紅利商品購買，請移除該品項後重新加入。";
                        response.Success = false;
                        continue;
                    }

                    if (requested > stock)
                    {
                        itemResult.Success = false;
                        itemResult.Error = "StockNotEnough";
                        itemResult.Message = $"此商品目前剩餘 {stock} 件，請調整購買數量。";
                        if (requested < original)
                        {
                            sc.Quantity = requested;
                        }
                        else
                        {
                            response.Success = false;
                        }
                        continue;
                    }

                    if ((sc.Bonus ?? 0) > 0 && requested > original)
                    {
                        var incrementQty = requested - original;
                        var incrementBonus = (sc.Bonus ?? 0) * incrementQty;

                        var bonusCheck = await CheckCartBonusEnough(uuid, incrementBonus);

                        if (!bonusCheck.IsEnough)
                        {
                            itemResult.Success = false;
                            itemResult.Error = "BonusNotEnough";
                            itemResult.Message = bonusCheck.Message;
                            response.Success = false;
                            continue;
                        }
                    }

                    sc.Quantity = requested;
                    sc.OldQuantity = original;
                    sc.LastModificationTime = DateTime.Now;
                    sc.LastModifierUserId = sc.CreatorUserId;

                    itemResult.NewQuantity = requested;
                    itemResult.Success = true;
                    itemResult.Message = "更新成功";

                    LogCartEventAsync(
                        pro_stock.FK_Pid,
                        sc.FK_Uid,
                        sc.UUID,
                        LogActionEnum.購物車數量變更,
                        original,
                        requested
                    );
                }

                await db.SaveChangesAsync();

                if (!response.Success)
                {
                    var firstError = batchResult.Items.FirstOrDefault(x => !x.Success);

                    if (firstError != null)
                    {
                        response.Error = firstError.Error ?? "商品更新失敗";
                        response.Message = firstError.Message ?? "商品更新失敗，請檢查列表訊息。";
                    }
                    else
                    {
                        response.Error = "部分商品更新失敗";
                        response.Message = "部分商品因庫存或資料問題未能更新，請檢查列表訊息。";
                    }
                }
                else
                {
                    response.Message = "更新成功。";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = "Error";
                response.Message = ex.Message;
            }

            return response;
        }
        private void LogCartEventAsync(long pid, long? userId, Guid uuid, LogActionEnum action, int before, int after)
        {
            db.Prod_Logs.Add(new Prod_Log
            {
                FK_Pid = pid,
                FK_UserId = userId,
                UUID = uuid,
                Action = action,
                Remark = $"before={before}, after={after}"
            });
        }
        public async Task<List<ShoppingCartDisplayDto>> GetAll()
        {
            List<ShoppingCartDisplayDto> output = new List<ShoppingCartDisplayDto>();
            try
            {
                Guid UUID = await tokenAppService.GetUUID();
                var scs = await db.ShoppingCarts.Where(e => e.UUID == UUID && !e.IsOrder).ToListAsync();
                if (scs.Any())
                {
                    var scids = scs.Select(e => e.Id).ToList();
                    output = await GetDisplay(scids);
                }
                else throw new Exception("查無購物車資料");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ShoppingCart=>GetDropOne回傳資料：{ex.Message}");
            }
            return output;
        }
        public async Task<ShoppingCartDisplayDto> GetDropOne(long id, bool isorder)
        {
            ShoppingCartDisplayDto output = new ShoppingCartDisplayDto();
            try
            {
                var shoppingcart = await db.ShoppingCarts.Where(e => e.Id == id && e.IsOrder == isorder).FirstOrDefaultAsync();
                if (shoppingcart != null)
                {
                    var temp_output = await GetDisplay(new List<long> { shoppingcart.Id });
                    if (temp_output.Any())
                    {
                        output = temp_output[0];
                    }
                    else throw new Exception("查無購物車資料");
                }
                else throw new Exception("查無購物車資料");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ShoppingCart=>GetDropOne回傳資料：{ex.Message}");
            }
            return output;
        }
        public async Task<List<ShoppingCartDisplayDto>> GetDisplay(List<long> scids)
        {
            List<ShoppingCartDisplayDto> output = new List<ShoppingCartDisplayDto>();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId") != 0 ? configuration.GetValue<long>("WebConfig:SiteId") : await loginUserData.GetWebsiteId();
            var Token = await tokenAppService.CheckToken(null);
            Guid UUID = await tokenAppService.GetUUID();
            long roleid = 1;

            try
            {
                var bonusSetting = await bonusManagementAppService.GetBonusSettingForEdit();
                var bonusEnabled = bonusSetting?.BonusEnabled == true;

                if (Token != null && Token.IsLogin)
                {
                    var temp_roleid = await db.MappingUserAndRoles.Where(e => e.UUID == UUID).Select(e => e.Id).FirstOrDefaultAsync();
                    if (temp_roleid != 0) roleid = temp_roleid;
                }

                var shoppingCarts = await db.ShoppingCarts
                    .Include(e => e.Prod_Stock)
                        .ThenInclude(e => e.Prod)
                            .ThenInclude(e => e.MappingLogisticsSettingAndProds)
                                .ThenInclude(e => e.LogisticsSetting)
                                    .ThenInclude(e => e.logisticsBoxFees)
                                        .ThenInclude(e => e.logisticsBox)
                    .Where(e => scids.Contains(e.Id))
                    .ToListAsync();

                foreach (var shoppingCart in shoppingCarts)
                {
                    var prod_price_id = await db.Prod_Prices.Where(e => e.Id == shoppingCart.FK_PriceId).Select(e => e.Id).FirstOrDefaultAsync();
                    if (prod_price_id == 0)
                    {
                        var prices_data = await productAppService.GetPriceDataAll(shoppingCart.FK_PSid);
                        shoppingCart.FK_PriceId = prices_data[0].Id;
                        await db.SaveChangesAsync();
                    }

                    var prod_stocks = shoppingCart.Prod_Stock;
                    var prods = prod_stocks.Prod;
                    var temp_output = mapper.Map<ShoppingCartDisplayDto>(shoppingCart);
                    var date_now = DateTime.Now;

                    temp_output.Available = prods.Visible && !prods.RemovedFromShelves && (prods.permanent || (date_now > prods.StartTime && date_now < prods.EndTime));
                    temp_output.Stock = prod_stocks?.Stock ?? 0;
                    temp_output.OldPrice = shoppingCart.Price;
                    temp_output.DynamicPrice = prod_stocks?.Price ?? 0;
                    temp_output.OldBonus = shoppingCart.Bonus ?? 0;

                    temp_output.Title = prods?.Title ?? "";
                    if (shoppingCart.IsOrder)
                    {
                        if (shoppingCart.ProdName != null && shoppingCart.ProdName != "")
                        {
                            temp_output.Title = shoppingCart.ProdName;
                        }
                        else
                        {
                            var sc = await db.ShoppingCarts.FirstOrDefaultAsync(e => e.Id == shoppingCart.Id);
                            if (sc != null)
                            {
                                sc.ProdName = temp_output.Title;
                                await db.SaveChangesAsync();
                            }
                        }
                    }
                    else
                    {
                        if (shoppingCart.ProdName != null && shoppingCart.ProdName != "")
                            temp_output.OldTitle = shoppingCart.ProdName;
                    }

                    var pid = prod_stocks?.Prod?.Id;
                    temp_output.PId = pid ?? 0;

                    var imagepath = await (from fu in db.FileUploads
                                           join fb in db.FileBinds on fu.Id equals fb.FK_FileUploadId
                                           where fb.Sid == pid && fb.type == (int)FileBindTypeEnum.產品
                                           where fu.FK_WebsiteId == WebsiteId
                                           where fu.ContentType.StartsWith("image")
                                           orderby fb.SerNo, fb.CreationTime
                                           select fu.DownloadFileName).FirstOrDefaultAsync();

                    temp_output.ImagePath = imagepath?.ToString() ?? "/images/noImg.jpg";
                    if (temp_output.ImagePath != "") temp_output.ImagePath = $"{temp_output.ImagePath}";

                    var db_sp = await db.Prod_Specs.ToListAsync();
                    if (db_sp.Any())
                    {
                        temp_output.S1Title = shoppingCart.Prod_Stock.FK_S1id != null ? db_sp.Find(e => e.Id == shoppingCart.Prod_Stock.FK_S1id)?.Title ?? "" : "";
                        temp_output.S2Title = shoppingCart.Prod_Stock.FK_S2id != null ? db_sp.Find(e => e.Id == shoppingCart.Prod_Stock.FK_S2id)?.Title ?? "" : "";
                    }

                    var psid = prod_stocks?.Id;
                    var prices = new List<ProductPriceDto>();
                    Prod_Price? prod_price = null;

                    if (psid != null)
                    {
                        prices = await productAppService.GetPriceByStock(new List<long> { (long)psid });
                    }

                    if (shoppingCart.FK_PriceId != null)
                    {
                        prod_price = await db.Prod_Prices
                            .FirstOrDefaultAsync(e => e.Id == shoppingCart.FK_PriceId);
                        temp_output.PPId = shoppingCart.FK_PriceId;
                    }

                    decimal currentPrice = temp_output.OldPrice;
                    int currentBonus = shoppingCart.Bonus ?? 0;

                    if (prices.Any() && prod_price != null)
                    {
                        var temp_price = prices
                            .FirstOrDefault(e => e.Id == prod_price.Id)
                            ?? prices.FirstOrDefault(e => e.Bonus == prod_price.Bonus)
                            ?? prices.FirstOrDefault();

                        if (temp_price != null)
                        {
                            if (temp_price.Id != prod_price.Id)
                            {
                                shoppingCart.FK_PriceId = temp_price.Id;
                                prod_price = await db.Prod_Prices
                                    .FirstOrDefaultAsync(e => e.Id == temp_price.Id);
                                await db.SaveChangesAsync();
                            }

                            currentPrice = prod_price?.Price ?? currentPrice;
                            currentBonus = prod_price?.Bonus ?? currentBonus;

                            if (prod_price?.FK_RId != 1)
                                temp_output.PriceLabel = "會員價";
                        }
                    }

                    if (!bonusEnabled && currentBonus > 0)
                    {
                        temp_output.Available = false;
                        temp_output.Quantity = 0;
                        temp_output.Describe = "目前未開放紅利商品購買，請移除該品項後重新選購。";
                    }

                    temp_output.Price = currentPrice;
                    temp_output.Bonus = currentBonus;
                    temp_output.PackingPoint = prod_stocks.PackingPoint;

                    if (!temp_output.Available) temp_output.Quantity = 0;

                    temp_output.Subtotal = temp_output.Price * temp_output.Quantity;
                    temp_output.SubtotalBonus = temp_output.Bonus * temp_output.Quantity;

                    if (string.IsNullOrWhiteSpace(temp_output.Describe))
                        temp_output.Describe = prods?.Description ?? "";

                    temp_output.Step = prod_stocks?.Min_Qty ?? 1;

                    output.Add(temp_output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ShoppingCart=>GetDisplay回傳資料：{ex.Message}");
            }

            return output;
        }
        public async Task<ResponseMessageDto> Reorder(List<long> scids)
        {
            ResponseMessageDto output = new ResponseMessageDto();
            var WebsiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var StockAllNull = true;

            try
            {
                var oldscs = await db.ShoppingCarts.Include(e => e.Prod_Stock).ThenInclude(e => e.Prod).Where(e => scids.Contains(e.Id)).ToListAsync();
                if (oldscs.Any())
                {
                    foreach (var oldsc in oldscs)
                    {
                        if (oldsc.Prod_Stock.Prod.Status != ProdStatusEnum.售完)
                        {
                            if (!oldsc.Prod_Stock.Prod.RemovedFromShelves && oldsc.Prod_Stock.Stock > 0)
                            {
                                ShoppingCartAddUpDto newsc = new ShoppingCartAddUpDto();
                                newsc = mapper.Map<ShoppingCartAddUpDto>(oldsc);
                                newsc.Id = null;
                                if (newsc.Quantity > oldsc.Prod_Stock.Stock) newsc.Quantity = (int)oldsc.Prod_Stock.Stock;
                                else newsc.Quantity = oldsc.Quantity;
                                var temp_response = await AddUp(newsc);
                                if (temp_response.Success) StockAllNull = false;
                                else throw new Exception(temp_response.Message);
                            }
                        }
                    }
                    if (!StockAllNull) output.Success = true;
                    else throw new Exception("訂單中商品皆已無庫存或已下架");
                }
                else throw new Exception("查無舊購物車資料");
            }
            catch (Exception ex)
            {
                output.Error = "Error";
                output.Message = ex.Message;
            }
            return output;
        }
        public async Task<List<ShoppingCartDisplayDto>> CheckStockPrice(List<long> scids)
        {
            List<ShoppingCartDisplayDto> output = new List<ShoppingCartDisplayDto>();
            try
            {
                var temp_outputs = await GetDisplay(scids);
                if (temp_outputs.Any())
                {
                    output = temp_outputs;
                }
                else throw new Exception("查無訂單資訊");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-------------錯誤訊息查看-------------");
                Console.WriteLine($"ShoppingCart=>CheckStockPrice回傳資料：{ex.Message}");
            }
            return output;
        }
        public async Task<ResponseMessageDto> DeleteDrop(long id)
        {
            ResponseMessageDto output = new ResponseMessageDto() { Success = false };

            try
            {
                var result = await db.ShoppingCarts.Where(e => e.Id == id).FirstOrDefaultAsync();

                if (result != null)
                {
                    var db_ps = await db.Prod_Stocks.Where(e => e.Id == result.FK_PSid).FirstOrDefaultAsync();
                    if (db_ps != null)
                    {
                        result.IsDeleted = true;
                        result.DeletionTime = DateTime.Now;
                        result.DeleterUserId = result.CreatorUserId;

                        db.SaveChanges();
                        output.Success = true;
                    }
                    else throw new Exception("查無商品資料");
                }
                else throw new Exception("查無購物車資料");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
    }
}

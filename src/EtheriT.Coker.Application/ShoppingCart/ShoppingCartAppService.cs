using AutoMapper;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Product;
using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.Member;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.Application.StoreSet;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EtheriT.Coker.Application.Shared.i18n;
using EtheriT.Coker.Application.Marketing;
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
        private readonly IStoreSetAppService storeSetAppService;
        private readonly IFrontRoleContextService frontRoleContextService;
        public ShoppingCartAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITokenAppService tokenAppService,
            IConfiguration configuration,
            IMapper mapper,
            IProductAppService productAppService,
            IBonusManagementAppService bonusManagementAppService,
            IStoreSetAppService storeSetsAppService,
            IFrontRoleContextService frontRoleContextService
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.tokenAppService = tokenAppService;
            this.configuration = configuration;
            this.mapper = mapper;
            this.productAppService = productAppService;
            this.bonusManagementAppService = bonusManagementAppService;
            this.storeSetAppService = storeSetsAppService;
            this.frontRoleContextService = frontRoleContextService;
        }
        public async Task<ResponseMessageDto> UpdateUUID(Guid UserUUID, Guid TempUUID)
        {
            var response = new ResponseMessageDto();
            try
            {
                var userid = await db.FrontUsers.Where(e => e.UUID == UserUUID).Select(e => e.FK_User).FirstOrDefaultAsync();
                // 登入只合併尚未結帳的購物車。已成立訂單必須保留原下單者 UUID。
                var tempsc = await db.ShoppingCarts
                    .Where(e => e.UUID == TempUUID && !e.IsOrder && !e.IsDeleted)
                    .ToListAsync();
                var usersc = await db.ShoppingCarts
                    .Where(e => e.UUID == UserUUID && !e.IsOrder && !e.IsDeleted)
                    .ToListAsync();
                var timenow = DateTime.Now;
                if (tempsc.Any())
                {
                    foreach (var tempCart in tempsc)
                    {
                        var userCart = usersc.FirstOrDefault(e =>
                            e.FK_PSid == tempCart.FK_PSid &&
                            e.IsAdditional == tempCart.IsAdditional &&
                            (!tempCart.IsAdditional ||
                             e.FK_MarketingRewardItemId == tempCart.FK_MarketingRewardItemId));
                        if (userCart != null)
                        {
                            userCart.Quantity += tempCart.Quantity;
                            userCart.LastModifierUserId = userid ?? 0;
                            userCart.LastModificationTime = timenow;
                            tempCart.IsDeleted = true;
                            tempCart.DeletionTime = timenow;
                            continue;
                        }

                        tempCart.UUID = UserUUID;
                        tempCart.FK_Uid = userid ?? 0;
                        tempCart.LastModifierUserId = userid ?? 0;
                        tempCart.LastModificationTime = timenow;
                    }

                    await db.SaveChangesAsync();
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
        public Task<ResponseMessageDto> AddUp(ShoppingCartAddUpDto dto)
        {
            return AddUpInternal(dto);
        }

        private async Task<ResponseMessageDto> AddUpInternal(
            ShoppingCartAddUpDto dto,
            Core.Models.ShoppingCart? sourceOrderSnapshot = null)
        {
            var rewardSelections = dto.RewardSelections?
                .Where(x => x.CampaignId > 0 && x.RewardItemId > 0 && x.Quantity > 0)
                .ToList() ?? new List<ShoppingCartRewardSelectionDto>();

            if (!rewardSelections.Any())
                return await ExecuteAddUpInternal(dto, sourceOrderSnapshot, rewardSelections);

            var strategy = db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(() =>
                ExecuteAddUpInternal(dto, sourceOrderSnapshot, rewardSelections));
        }

        private async Task<ResponseMessageDto> ExecuteAddUpInternal(
            ShoppingCartAddUpDto dto,
            Core.Models.ShoppingCart? sourceOrderSnapshot,
            List<ShoppingCartRewardSelectionDto> rewardSelections)
        {
            ResponseMessageDto response = new ResponseMessageDto() { Success = false };
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null;

            try
            {
                if (rewardSelections.Any())
                    transaction = await db.Database.BeginTransactionAsync();

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
                        throw new Exception(L.get("TokenError"));
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
                    throw new Exception(L.get("SpecNotFound"));

                var prod = await db.Prods.FirstOrDefaultAsync(e => e.Id == proStock.FK_Pid && !e.RemovedFromShelves);
                if(prod == null) throw new Exception(L.get("ProductUnavailable"));
                else if(IsCantBuyProdState(prod)) throw new Exception(L.get("ProdEmpty"));

                var skipStock = prod.NoStockManagement;

                var currentStock = proStock.Stock ?? 0;
                if (currentStock <= 0 && !skipStock)
                    throw new Exception(L.get("OutOfStock"));

                var specIds = new[] { proStock.FK_S1id, proStock.FK_S2id }
                    .Where(id => id.HasValue && id.Value > 0)
                    .Select(id => id!.Value)
                    .Distinct()
                    .ToList();
                var specTitles = specIds.Any()
                    ? await db.Prod_Specs
                        .Where(spec => specIds.Contains(spec.Id))
                        .ToDictionaryAsync(spec => spec.Id, spec => spec.Title)
                    : new Dictionary<long, string>();
                var s1SnapshotTitle = proStock.FK_S1id.HasValue && specTitles.TryGetValue(proStock.FK_S1id.Value, out var currentS1Title)
                    ? currentS1Title
                    : null;
                var s2SnapshotTitle = proStock.FK_S2id.HasValue && specTitles.TryGetValue(proStock.FK_S2id.Value, out var currentS2Title)
                    ? currentS2Title
                    : null;
                var preserveOrderSnapshot = sourceOrderSnapshot != null;
                var cartS1Id = preserveOrderSnapshot ? sourceOrderSnapshot!.FK_S1id : proStock.FK_S1id;
                var cartS2Id = preserveOrderSnapshot ? sourceOrderSnapshot!.FK_S2id : proStock.FK_S2id;
                var cartS1Title = preserveOrderSnapshot ? sourceOrderSnapshot!.S1Title : s1SnapshotTitle;
                var cartS2Title = preserveOrderSnapshot ? sourceOrderSnapshot!.S2Title : s2SnapshotTitle;
                var cartProductId = preserveOrderSnapshot
                    ? sourceOrderSnapshot!.ProductId ?? prod.Id
                    : prod.Id;
                var cartProdName = preserveOrderSnapshot && !string.IsNullOrWhiteSpace(sourceOrderSnapshot!.ProdName)
                    ? sourceOrderSnapshot.ProdName
                    : !string.IsNullOrWhiteSpace(dto.ProdName) ? dto.ProdName : prod.Title;

                Core.Models.ShoppingCart? sc = null;
                if (dto.Id != null)
                {
                    sc = await db.ShoppingCarts.FirstOrDefaultAsync(e => e.Id == dto.Id);
                    if (sc == null) throw new Exception((L.get("CartNotFound")));
                }
                else
                {
                    sc = await db.ShoppingCarts.FirstOrDefaultAsync(e =>
                        e.UUID == UUID &&
                        e.FK_PSid == proStock.Id &&
                        e.FK_PriceId == dto.FK_PriceId &&
                        !e.IsAdditional &&
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
                        throw new Exception(L.get("PleaseSignIn"));

                    var incrementBonus = bonus * wantQty;
                    var bonusCheck = await CheckCartBonusEnough(UUID, incrementBonus);

                    if (!bonusCheck.IsEnough)
                        throw new Exception(bonusCheck.Message);
                }
                // ===== 紅利檢查結束 =====

                var isNewCart = sc == null;
                if (sc == null)
                {
                    if (wantQty > currentStock && !skipStock)
                        throw new Exception(L.get("CartLimitExceeded", currentStock, wantQty));

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
                        FK_S1id = cartS1Id,
                        FK_S2id = cartS2Id,
                        ProductId = cartProductId,
                        ProdName = cartProdName,
                        S1Title = cartS1Title,
                        S2Title = cartS2Title,
                        CreatorUserId = userid,
                        CreationTime = date
                    };

                    db.ShoppingCarts.Add(sc);
                    LogCartEventAsync(proStock.FK_Pid, userid, UUID, LogActionEnum.加入購物車, 0, wantQty);
                }
                else
                {
                    int newTotal = sc.Quantity + wantQty;
                    int oQuantity = sc.Quantity;

                    if (newTotal > currentStock && !skipStock)
                        throw new Exception(L.get("CartLimitExceededWithCart", currentStock, sc.Quantity, wantQty));

                    sc.Quantity = newTotal;
                    sc.Price = unitPrice;
                    sc.Bonus = bonus;
                    sc.FK_PriceId = dto.FK_PriceId;
                    sc.ProductId = cartProductId;
                    sc.S1Title = cartS1Title;
                    sc.S2Title = cartS2Title;
                    sc.LastModificationTime = DateTime.Now;
                    sc.LastModifierUserId = userid;

                    if (preserveOrderSnapshot)
                    {
                        sc.FK_S1id = cartS1Id;
                        sc.FK_S2id = cartS2Id;
                        sc.ProdName = cartProdName;
                    }
                    else if (sc.FK_S1id == null && sc.FK_S2id == null)
                    {
                        sc.FK_S1id = proStock.FK_S1id;
                        sc.FK_S2id = proStock.FK_S2id;
                    }

                    if (string.IsNullOrWhiteSpace(sc.ProdName))
                    {
                        sc.ProdName = prod.Title;
                    }

                    LogCartEventAsync(proStock.FK_Pid, userid, UUID, LogActionEnum.加入購物車, oQuantity, newTotal);

                }

                var operation = isNewCart ? "N" : "U";
                await db.SaveChangesAsync();
                response.Message = operation + sc.Id;

                if (rewardSelections.Any())
                {
                    response.Object = await AddMarketingRewardItemsAsync(
                        UUID,
                        userid,
                        prod.FK_WebsiteId,
                        proStock.FK_Pid,
                        rewardSelections);
                    await db.SaveChangesAsync();
                }

                if (transaction != null)
                    await transaction.CommitAsync();

                response.Success = true;
            }
            catch (Exception ex)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();
                response.Error = "Error";
                response.Message = ex.Message;
            }
            finally
            {
                if (transaction != null)
                    await transaction.DisposeAsync();
            }

            return response;
        }

        private async Task<List<long>> AddMarketingRewardItemsAsync(
            Guid uuid,
            long userId,
            long websiteId,
            long purchasedProductId,
            List<ShoppingCartRewardSelectionDto> selections)
        {
            var now = DateTime.Now;
            var campaignIds = selections.Select(x => x.CampaignId).Distinct().ToList();
            var campaigns = await db.MarketingCampaigns
                .Include(x => x.Rules)
                    .ThenInclude(x => x.Condition)
                .Include(x => x.Rules)
                    .ThenInclude(x => x.ScopeItems)
                .Include(x => x.Rules)
                    .ThenInclude(x => x.Reward)
                        .ThenInclude(x => x.Items)
                            .ThenInclude(x => x.ProdStock)
                                .ThenInclude(x => x.Prod)
                .Where(x => campaignIds.Contains(x.Id) && !x.IsDeleted && x.FK_WebsiteId == websiteId)
                .Where(x => x.Status == MarketingDisplayStatusEnum.活動中 && x.StartTime <= now)
                .Where(x => x.NeverEnd || (x.EndTime.HasValue && x.EndTime.Value >= now))
                .ToListAsync();

            if (campaigns.Count != campaignIds.Count)
                throw new Exception("部分加價購活動已失效，請重新整理商品頁後再試。");

            var currentCarts = await db.ShoppingCarts
                .Include(x => x.Prod_Stock)
                .Where(x => x.UUID == uuid && !x.IsOrder && !x.IsDeleted)
                .ToListAsync();
            var affectedCartIds = new List<long>();

            foreach (var campaignId in campaignIds)
            {
                var campaign = campaigns.Single(x => x.Id == campaignId);
                var campaignSelections = selections.Where(x => x.CampaignId == campaignId).ToList();
                var rule = campaign.Rules
                    .Where(x => !x.IsDeleted && x.Enabled &&
                                x.RuleType == MarketingRuleTypeEnum.AddOnPurchase &&
                                x.Condition != null && !x.Condition.IsDeleted &&
                                x.Reward != null && !x.Reward.IsDeleted)
                    .OrderBy(x => x.SortOrder)
                    .FirstOrDefault();

                if (rule == null)
                    throw new Exception("加價購活動規則已失效，請重新整理商品頁後再試。");

                var scopeProductIds = rule.ScopeItems
                    .Where(x => !x.IsDeleted && x.TargetType == MarketingScopeTargetTypeEnum.Product)
                    .Select(x => x.TargetId)
                    .Distinct()
                    .ToHashSet();
                if (!scopeProductIds.Contains(purchasedProductId))
                    throw new Exception("目前商品不適用此加價購活動。");

                var requiredQuantity = Math.Max(rule.Condition.MinQuantity ?? 1, 1);
                var qualifyingQuantity = currentCarts
                    .Where(x => !x.IsAdditional && x.Prod_Stock != null &&
                                scopeProductIds.Contains(x.Prod_Stock.FK_Pid))
                    .Sum(x => x.Quantity);
                var qualificationCount = campaign.Repeatable
                    ? qualifyingQuantity / requiredQuantity
                    : qualifyingQuantity >= requiredQuantity ? 1 : 0;
                var allowance = qualificationCount * Math.Max(rule.Reward.SelectionQuantityPerQualification, 1);
                if (allowance <= 0)
                    throw new Exception($"尚未達到「{campaign.Name}」的加價購條件。");

                var activeRewardItems = rule.Reward.Items
                    .Where(x => !x.IsDeleted && x.Enabled && x.ProdStock != null && !x.ProdStock.IsDeleted &&
                                x.ProdStock.Prod != null && !x.ProdStock.Prod.IsDeleted &&
                                x.ProdStock.Prod.Visible && !x.ProdStock.Prod.RemovedFromShelves)
                    .ToDictionary(x => x.Id);
                if (campaignSelections.Any(x => !activeRewardItems.ContainsKey(x.RewardItemId)))
                    throw new Exception("選取的加價購商品已失效，請重新選擇。");

                var rewardItemIds = activeRewardItems.Keys.ToHashSet();
                var rewardStockIds = activeRewardItems.Values.Select(x => x.FK_ProdStockId).ToHashSet();
                var existingRewardQuantity = currentCarts
                    .Where(x => x.IsAdditional &&
                        (x.FK_MarketingRewardItemId.HasValue
                            ? rewardItemIds.Contains(x.FK_MarketingRewardItemId.Value)
                            : rewardStockIds.Contains(x.FK_PSid)))
                    .Sum(x => x.Quantity);
                var requestedQuantity = campaignSelections.Sum(x => x.Quantity);
                if (existingRewardQuantity + requestedQuantity > allowance)
                    throw new Exception($"「{campaign.Name}」目前最多可選 {allowance} 件優惠商品。");

                foreach (var selection in campaignSelections)
                {
                    var rewardItem = activeRewardItems[selection.RewardItemId];
                    var stock = rewardItem.ProdStock;
                    var existingItemQuantity = currentCarts
                        .Where(x => x.IsAdditional &&
                            (x.FK_MarketingRewardItemId == rewardItem.Id ||
                             (!x.FK_MarketingRewardItemId.HasValue && x.FK_PSid == stock.Id)))
                        .Sum(x => x.Quantity);
                    var itemLimit = Math.Max(rewardItem.MaxQuantityPerOrder, 1) *
                                    (campaign.Repeatable ? qualificationCount : 1);
                    if (existingItemQuantity + selection.Quantity > itemLimit)
                        throw new Exception($"「{stock.Prod!.Title}」超過此活動可選上限。");
                    if (!stock.Prod.NoStockManagement && (stock.Stock ?? 0) < existingItemQuantity + selection.Quantity)
                        throw new Exception($"「{stock.Prod.Title}」庫存不足。");

                    var cart = currentCarts
                        .Where(x => x.IsAdditional &&
                            (x.FK_MarketingRewardItemId == rewardItem.Id ||
                             (!x.FK_MarketingRewardItemId.HasValue && x.FK_PSid == stock.Id)))
                        .OrderBy(x => x.CreationTime)
                        .FirstOrDefault();
                    if (cart == null)
                    {
                        var specIds = new[] { stock.FK_S1id, stock.FK_S2id }
                            .Where(x => x.HasValue && x.Value > 0)
                            .Select(x => x!.Value)
                            .Distinct()
                            .ToList();
                        var specTitles = specIds.Any()
                            ? await db.Prod_Specs.Where(x => specIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Title)
                            : new Dictionary<long, string>();
                        cart = new Core.Models.ShoppingCart
                        {
                            FK_PSid = stock.Id,
                            FK_PriceId = null,
                            Price = rewardItem.OfferPrice,
                            Bonus = 0,
                            Quantity = selection.Quantity,
                            FK_Tid = currentCarts.Select(x => x.FK_Tid).FirstOrDefault(),
                            FK_Uid = userId,
                            UUID = uuid,
                            Ser_No = 500,
                            FK_S1id = stock.FK_S1id,
                            FK_S2id = stock.FK_S2id,
                            ProductId = stock.FK_Pid,
                            ProdName = stock.Prod.Title,
                            S1Title = stock.FK_S1id.HasValue && specTitles.TryGetValue(stock.FK_S1id.Value, out var s1) ? s1 : null,
                            S2Title = stock.FK_S2id.HasValue && specTitles.TryGetValue(stock.FK_S2id.Value, out var s2) ? s2 : null,
                            IsAdditional = true,
                            FK_MarketingRewardItemId = rewardItem.Id,
                            CreatorUserId = userId,
                            CreationTime = now
                        };
                        db.ShoppingCarts.Add(cart);
                        currentCarts.Add(cart);
                    }
                    else
                    {
                        cart.FK_MarketingRewardItemId ??= rewardItem.Id;
                        cart.Quantity += selection.Quantity;
                        cart.LastModifierUserId = userId;
                        cart.LastModificationTime = now;
                    }

                    await db.SaveChangesAsync();
                    affectedCartIds.Add(cart.Id);
                }
            }

            return affectedCartIds.Distinct().ToList();
        }

        public async Task<ResponseMessageDto> UpdateAddOnSelections(ShoppingCartAddOnSelectionUpdateDto dto)
        {
            var response = new ResponseMessageDto();
            try
            {
                var uuid = await tokenAppService.GetUUID();
                var websiteId = await loginUserData.GetCommonWebsiteId();
                var now = DateTime.Now;

                var campaign = await db.MarketingCampaigns
                    .Include(x => x.Rules).ThenInclude(x => x.Condition)
                    .Include(x => x.Rules).ThenInclude(x => x.ScopeItems)
                    .Include(x => x.Rules).ThenInclude(x => x.Reward).ThenInclude(x => x.Items)
                        .ThenInclude(x => x.ProdStock).ThenInclude(x => x.Prod)
                    .FirstOrDefaultAsync(x => x.Id == dto.CampaignId && x.FK_WebsiteId == websiteId &&
                        !x.IsDeleted && x.Status == MarketingDisplayStatusEnum.活動中 && x.StartTime <= now &&
                        (x.NeverEnd || (x.EndTime.HasValue && x.EndTime.Value >= now)));
                var rule = campaign?.Rules.FirstOrDefault(x => x.Id == dto.RuleId && !x.IsDeleted && x.Enabled &&
                    x.RuleType == MarketingRuleTypeEnum.AddOnPurchase && x.Condition != null && !x.Condition.IsDeleted &&
                    x.Reward != null && !x.Reward.IsDeleted);
                if (campaign == null || rule == null)
                    throw new Exception("加價購活動已失效，請重新整理購物車後再試。");

                var carts = await db.ShoppingCarts
                    .Include(x => x.Prod_Stock)
                    .Where(x => x.UUID == uuid && !x.IsOrder && !x.IsDeleted)
                    .OrderBy(x => x.CreationTime)
                    .ToListAsync();
                var baseCarts = carts.Where(x => !x.IsAdditional && x.Prod_Stock != null).ToList();
                var scopeProductIds = rule.ScopeItems
                    .Where(x => !x.IsDeleted && x.TargetType == MarketingScopeTargetTypeEnum.Product)
                    .Select(x => x.TargetId).Distinct().ToHashSet();

                var conditionType = rule.Condition.ConditionType;
                var qualificationCount = 0;
                if (conditionType == MarketingConditionTypeEnum.ScopeQuantity ||
                    conditionType == MarketingConditionTypeEnum.BuySpecificProduct)
                {
                    var requiredQuantity = Math.Max(rule.Condition.MinQuantity ?? 1, 1);
                    var quantity = baseCarts.Where(x => scopeProductIds.Contains(x.Prod_Stock!.FK_Pid)).Sum(x => x.Quantity);
                    qualificationCount = campaign.Repeatable ? quantity / requiredQuantity : quantity >= requiredQuantity ? 1 : 0;
                }
                else if (conditionType == MarketingConditionTypeEnum.OrderAmount ||
                         conditionType == MarketingConditionTypeEnum.ScopeAmount)
                {
                    var requiredAmount = Math.Max(rule.Condition.MinAmount ?? 0, 0);
                    var amount = baseCarts
                        .Where(x => conditionType != MarketingConditionTypeEnum.ScopeAmount || scopeProductIds.Contains(x.Prod_Stock!.FK_Pid))
                        .Sum(x => x.Price * x.Quantity);
                    qualificationCount = requiredAmount > 0
                        ? campaign.Repeatable ? (int)Math.Floor(amount / requiredAmount) : amount >= requiredAmount ? 1 : 0
                        : 0;
                }

                var allowance = qualificationCount * Math.Max(rule.Reward.SelectionQuantityPerQualification, 1);
                var desired = (dto.Selections ?? new List<ShoppingCartRewardSelectionDto>())
                    .Where(x => x.Quantity > 0)
                    .GroupBy(x => x.RewardItemId)
                    .ToDictionary(x => x.Key, x => x.Sum(y => y.Quantity));
                if (desired.Values.Sum() > allowance)
                    throw new Exception($"「{campaign.Name}」目前最多可選 {allowance} 件優惠商品。");

                var activeItems = rule.Reward.Items
                    .Where(x => !x.IsDeleted && x.Enabled && x.ProdStock != null && !x.ProdStock.IsDeleted &&
                        x.ProdStock.Prod != null && !x.ProdStock.Prod.IsDeleted && x.ProdStock.Prod.Visible &&
                        !x.ProdStock.Prod.RemovedFromShelves)
                    .ToDictionary(x => x.Id);
                if (desired.Keys.Any(x => !activeItems.ContainsKey(x)))
                    throw new Exception("選取的優惠商品已失效，請重新選擇。");

                var userId = await db.FrontUsers.Where(x => x.UUID == uuid).Select(x => x.FK_User).FirstOrDefaultAsync() ?? 0;
                foreach (var pair in activeItems)
                {
                    var rewardItem = pair.Value;
                    var wanted = desired.TryGetValue(pair.Key, out var qty) ? qty : 0;
                    var itemLimit = Math.Max(rewardItem.MaxQuantityPerOrder, 1) *
                        (campaign.Repeatable ? Math.Max(qualificationCount, 1) : 1);
                    if (wanted > itemLimit)
                        throw new Exception($"「{rewardItem.ProdStock!.Prod!.Title}」超過此活動可選上限。");
                    if (!rewardItem.ProdStock.Prod.NoStockManagement && (rewardItem.ProdStock.Stock ?? 0) < wanted)
                        throw new Exception($"「{rewardItem.ProdStock.Prod.Title}」庫存不足。");

                    var matching = carts
                        .Where(x => x.IsAdditional &&
                            (x.FK_MarketingRewardItemId == rewardItem.Id ||
                             (!x.FK_MarketingRewardItemId.HasValue && x.FK_PSid == rewardItem.FK_ProdStockId)))
                        .OrderBy(x => x.CreationTime)
                        .ToList();
                    var cart = matching.FirstOrDefault();
                    foreach (var duplicate in matching.Skip(1))
                    {
                        duplicate.IsDeleted = true;
                        duplicate.DeletionTime = now;
                        duplicate.DeleterUserId = userId;
                    }

                    if (wanted <= 0)
                    {
                        if (cart != null)
                        {
                            cart.IsDeleted = true;
                            cart.DeletionTime = now;
                            cart.DeleterUserId = userId;
                        }
                        continue;
                    }

                    if (cart == null)
                    {
                        var stock = rewardItem.ProdStock;
                        var specIds = new[] { stock.FK_S1id, stock.FK_S2id }.Where(x => x.HasValue && x.Value > 0)
                            .Select(x => x!.Value).Distinct().ToList();
                        var specTitles = specIds.Any()
                            ? await db.Prod_Specs.Where(x => specIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Title)
                            : new Dictionary<long, string>();
                        cart = new Core.Models.ShoppingCart
                        {
                            FK_PSid = stock.Id, FK_PriceId = null, Price = rewardItem.OfferPrice, Bonus = 0,
                            Quantity = wanted, FK_Tid = baseCarts.Select(x => x.FK_Tid).FirstOrDefault(),
                            FK_Uid = userId, UUID = uuid, Ser_No = 500, FK_S1id = stock.FK_S1id,
                            FK_S2id = stock.FK_S2id, ProductId = stock.FK_Pid, ProdName = stock.Prod.Title,
                            S1Title = stock.FK_S1id.HasValue && specTitles.TryGetValue(stock.FK_S1id.Value, out var s1) ? s1 : null,
                            S2Title = stock.FK_S2id.HasValue && specTitles.TryGetValue(stock.FK_S2id.Value, out var s2) ? s2 : null,
                            IsAdditional = true, FK_MarketingRewardItemId = rewardItem.Id,
                            CreatorUserId = userId, CreationTime = now
                        };
                        db.ShoppingCarts.Add(cart);
                    }
                    else
                    {
                        cart.FK_MarketingRewardItemId ??= rewardItem.Id;
                        cart.Quantity = wanted;
                        cart.LastModifierUserId = userId;
                        cart.LastModificationTime = now;
                    }
                }

                await db.SaveChangesAsync();
                response.Success = true;
                response.Message = "優惠商品已更新。";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = "AddOnSelectionUpdateFailed";
                response.Message = ex.Message;
            }
            return response;
        }
        private bool IsCantBuyProdState(Prod prod) {
            return !(prod.Status != ProdStatusEnum.售完 && prod.Status != ProdStatusEnum.停產 && !prod.RemovedFromShelves);
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
                    .Include(s => s.Prod)
                    .Where(s => stockIds.Contains(s.Id))
                    .ToListAsync();
                var specIds = stocks
                    .SelectMany(stock => new[] { stock.FK_S1id, stock.FK_S2id })
                    .Where(id => id.HasValue && id.Value > 0)
                    .Select(id => id!.Value)
                    .Distinct()
                    .ToList();
                var specTitles = specIds.Any()
                    ? await db.Prod_Specs
                        .Where(spec => specIds.Contains(spec.Id))
                        .ToDictionaryAsync(spec => spec.Id, spec => spec.Title)
                    : new Dictionary<long, string>();

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

                    var currentS1Title = pro_stock.FK_S1id.HasValue && specTitles.TryGetValue(pro_stock.FK_S1id.Value, out var s1Title)
                        ? s1Title
                        : null;
                    var currentS2Title = pro_stock.FK_S2id.HasValue && specTitles.TryGetValue(pro_stock.FK_S2id.Value, out var s2Title)
                        ? s2Title
                        : null;
                    var specCheck = ApplyCartSpecValidation(sc, pro_stock, currentS1Title, currentS2Title);
                    if (!specCheck.Success && specCheck.Error != "SpecTitleChanged")
                    {
                        itemResult.Success = false;
                        itemResult.Error = specCheck.Error;
                        itemResult.Message = specCheck.Message;
                        itemResult.OldQuantity = sc.Quantity;
                        itemResult.NewQuantity = sc.Quantity;
                        response.Success = false;
                        continue;
                    }

                    var stock = pro_stock.Stock ?? 0;
                    var skipStock = pro_stock.Prod?.NoStockManagement == true;
                    var requested = dto.Quantity;
                    var original = sc.Quantity;

                    itemResult.OldQuantity = original;
                    itemResult.NewQuantity = original;
                    itemResult.Removed = false;

                    if (stock <= 0 && !skipStock)
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

                    if (sc.IsAdditional && requested > original)
                    {
                        itemResult.Success = false;
                        itemResult.Error = "AdditionalQuantityLocked";
                        itemResult.Message = "加價購／贈品數量須回到商品頁依活動資格選取。";
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

                    if (requested > stock && !skipStock)
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
                    roleid = await frontRoleContextService.GetRoleIdAsync(UUID, WebsiteId);
                }

                var shoppingCarts = await db.ShoppingCarts
                    .Include(e => e.Prod_Stock)
                        .ThenInclude(e => e.Prod)
                            .ThenInclude(e => e.MappingLogisticsSettingAndProds)
                                .ThenInclude(e => e.LogisticsSetting)
                                    .ThenInclude(e => e.logisticsBoxFees)
                                        .ThenInclude(e => e.logisticsBox)
                    .Where(e => scids.Contains(e.Id))
                    .OrderBy(e => e.CreationTime)
                    .ThenBy(e => e.Id)
                    .ToListAsync();
                var validAdditionalCartPrices = await GetValidAdditionalCartPricesAsync(
                    shoppingCarts.Where(x => !x.IsOrder).ToList(),
                    WebsiteId,
                    DateTime.Now);

                var layoutSetting = await storeSetAppService.getValues(new Shared.Dto.StoreSet.StoreSetGetValueInput
                {
                    key = "ProductPageLayout",
                    SiteId = WebsiteId
                });
                var isLayout2 = layoutSetting?.detailItem?.value?.FirstOrDefault() == "Layout_2";

                foreach (var shoppingCart in shoppingCarts)
                {
                    var prod_price_id = await db.Prod_Prices.Where(e => e.Id == shoppingCart.FK_PriceId).Select(e => e.Id).FirstOrDefaultAsync();
                    if (!shoppingCart.IsAdditional && prod_price_id == 0)
                    {
                        var prices_data = await productAppService.GetPriceDataAll(shoppingCart.FK_PSid);
                        shoppingCart.FK_PriceId = prices_data[0].Id;
                        await db.SaveChangesAsync();
                    }

                    var prod_stocks = shoppingCart.Prod_Stock;
                    var prods = prod_stocks.Prod;
                    var temp_output = mapper.Map<ShoppingCartDisplayDto>(shoppingCart);
                    var date_now = DateTime.Now;

                    temp_output.Available = prods.Visible
                        && !IsCantBuyProdState(prods)
                        && (prods.permanent || (date_now > prods.StartTime && date_now < prods.EndTime));
                    temp_output.Stock = prod_stocks?.Stock ?? 0;
                    temp_output.NoStockManagement = prods?.NoStockManagement == true;

                    if (!shoppingCart.IsOrder)
                    {
                        if (!temp_output.Available)
                        {
                            temp_output.ValidationCode = "ProductUnavailable";
                            temp_output.Describe = "此商品目前已下架或無法購買，請移除該品項。";
                        }
                        else if (temp_output.Stock <= 0 && prods?.NoStockManagement != true)
                        {
                            temp_output.Available = false;
                            temp_output.ValidationCode = "StockNotEnough";
                            temp_output.Describe = "此商品目前已無庫存，請移除該品項或稍後再試。";
                        }

                        if (shoppingCart.IsAdditional && !validAdditionalCartPrices.ContainsKey(shoppingCart.Id))
                        {
                            temp_output.Available = false;
                            temp_output.ValidationCode = "AdditionalCampaignEnded";
                            temp_output.Describe = "此加價購／贈品活動已結束，或目前購物車已不符合活動資格，請移除該品項。";
                        }
                    }
                    temp_output.OldPrice = shoppingCart.Price;
                    temp_output.DynamicPrice = prod_stocks?.Price ?? 0;
                    temp_output.OldBonus = shoppingCart.Bonus ?? 0;
                    temp_output.IsAdditional = shoppingCart.IsAdditional;
                    if (shoppingCart.IsAdditional)
                        temp_output.PriceLabel = temp_output.ValidationCode == "AdditionalCampaignEnded"
                            ? "活動已結束"
                            : shoppingCart.Price <= 0 ? "贈品" : "加價購";

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

                    // 規格圖優先：買的是哪個規格就顯示哪個規格的圖
                    string? imagepath = null;

                    if (isLayout2 && prod_stocks != null)
                    {
                        imagepath = await (from fu in db.FileUploads
                                           join fb in db.FileBinds on fu.Id equals fb.FK_FileUploadId
                                           where fb.Sid == prod_stocks.Id && fb.type == (int)FileBindTypeEnum.產品規格圖
                                           where fu.FK_WebsiteId == WebsiteId
                                           where fu.ContentType.StartsWith("image")
                                           orderby fb.SerNo, fb.CreationTime
                                           select fu.DownloadFileName).FirstOrDefaultAsync();
                    }

                    // 該規格沒設圖 → 退回商品主圖
                    if (imagepath == null)
                    {
                        imagepath = await (from fu in db.FileUploads
                                           join fb in db.FileBinds on fu.Id equals fb.FK_FileUploadId
                                           where fb.Sid == pid && fb.type == (int)FileBindTypeEnum.產品
                                           where fu.FK_WebsiteId == WebsiteId
                                           where fu.ContentType.StartsWith("image")
                                           orderby fb.SerNo, fb.CreationTime
                                           select fu.DownloadFileName).FirstOrDefaultAsync();
                    }

                    temp_output.ImagePath = imagepath?.ToString() ?? "/images/noImg.jpg";
                    if (temp_output.ImagePath != "") temp_output.ImagePath = $"{temp_output.ImagePath}";

                    var currentSpecIds = new[] { prod_stocks.FK_S1id, prod_stocks.FK_S2id }
                        .Where(id => id.HasValue && id.Value > 0)
                        .Select(id => id!.Value)
                        .Distinct()
                        .ToList();
                    var currentSpecTitles = currentSpecIds.Any()
                        ? await db.Prod_Specs
                            .Where(spec => currentSpecIds.Contains(spec.Id))
                            .ToDictionaryAsync(spec => spec.Id, spec => spec.Title)
                        : new Dictionary<long, string>();
                    var currentS1Title = prod_stocks.FK_S1id.HasValue && currentSpecTitles.TryGetValue(prod_stocks.FK_S1id.Value, out var s1Title)
                        ? s1Title
                        : "";
                    var currentS2Title = prod_stocks.FK_S2id.HasValue && currentSpecTitles.TryGetValue(prod_stocks.FK_S2id.Value, out var s2Title)
                        ? s2Title
                        : "";

                    temp_output.S1Title = shoppingCart.IsOrder ? shoppingCart.S1Title ?? "" : currentS1Title;
                    temp_output.S2Title = shoppingCart.IsOrder ? shoppingCart.S2Title ?? "" : currentS2Title;

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

                    if (shoppingCart.IsAdditional &&
                        validAdditionalCartPrices.TryGetValue(shoppingCart.Id, out var currentOfferPrice))
                    {
                        currentPrice = currentOfferPrice;
                    }

                    if (!shoppingCart.IsAdditional && prices.Any() && prod_price != null)
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
                    if (shoppingCart.IsAdditional)
                    {
                        temp_output.PriceLabel = temp_output.ValidationCode == "AdditionalCampaignEnded"
                            ? "活動已結束"
                            : currentPrice <= 0 ? "贈品" : "加價購";
                    }

                    var specCheck = shoppingCart.IsOrder || !temp_output.Available
                        ? new ResponseMessageDto { Success = true }
                        : ApplyCartSpecValidation(shoppingCart, prod_stocks, currentS1Title, currentS2Title);
                    var productTitleChanged = !shoppingCart.IsOrder
                        && temp_output.Available
                        && !string.IsNullOrWhiteSpace(shoppingCart.ProdName)
                        && !string.Equals(shoppingCart.ProdName, prods.Title, StringComparison.Ordinal);
                    var specTitleChanged = !specCheck.Success && specCheck.Error == "SpecTitleChanged";

                    if (productTitleChanged || specTitleChanged)
                    {
                        var changeMessages = new List<string>();
                        if (productTitleChanged)
                        {
                            changeMessages.Add($"商品名稱：由「{shoppingCart.ProdName}」調整為「{prods.Title}」");
                        }
                        if (specTitleChanged && !string.IsNullOrWhiteSpace(specCheck.Message))
                        {
                            changeMessages.Add(specCheck.Message);
                        }

                        temp_output.ValidationCode = productTitleChanged && specTitleChanged
                            ? "CartSnapshotChanged"
                            : productTitleChanged ? "ProductTitleChanged" : "SpecTitleChanged";
                        temp_output.Describe = $"商品資訊已有異動：{string.Join("；", changeMessages)}。請確認後再勾選結帳。";
                    }
                    else if (!specCheck.Success)
                    {
                        temp_output.ValidationCode = specCheck.Error;
                        temp_output.Describe = specCheck.Message;
                        temp_output.Available = false;
                        temp_output.Quantity = 0;
                    }

                    if (!temp_output.Available && temp_output.ValidationCode != "AdditionalCampaignEnded")
                        temp_output.Quantity = 0;

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

            var rewardInfos = await MarketingCartOrdering.LoadRewardInfosAsync(
                db,
                output.Select(x => x.FK_MarketingRewardItemId));
            return MarketingCartOrdering.Sort(
                output,
                x => x.IsAdditional,
                x => x.PId,
                x => x.FK_MarketingRewardItemId,
                rewardInfos);
        }

        private async Task<Dictionary<long, decimal>> GetValidAdditionalCartPricesAsync(
            List<Core.Models.ShoppingCart> carts,
            long websiteId,
            DateTime now)
        {
            var validPrices = new Dictionary<long, decimal>();
            var baseCarts = carts.Where(x => !x.IsAdditional && x.Prod_Stock != null).ToList();
            var additionalCarts = carts.Where(x => x.IsAdditional).OrderBy(x => x.CreationTime).ToList();
            if (!additionalCarts.Any())
                return validPrices;

            var rewardStockIds = additionalCarts.Select(x => x.FK_PSid).Distinct().ToList();
            var rewardItemIds = additionalCarts
                .Where(x => x.FK_MarketingRewardItemId.HasValue)
                .Select(x => x.FK_MarketingRewardItemId!.Value)
                .Distinct()
                .ToList();
            var campaigns = await db.MarketingCampaigns
                .AsNoTracking()
                .Include(x => x.Rules).ThenInclude(x => x.Condition)
                .Include(x => x.Rules).ThenInclude(x => x.ScopeItems)
                .Include(x => x.Rules).ThenInclude(x => x.Reward).ThenInclude(x => x.Items)
                .Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted &&
                    x.Status == MarketingDisplayStatusEnum.活動中 && x.StartTime <= now &&
                    (x.NeverEnd || (x.EndTime.HasValue && x.EndTime.Value >= now)) &&
                    x.Rules.Any(r => !r.IsDeleted && r.Enabled &&
                        r.RuleType == MarketingRuleTypeEnum.AddOnPurchase && r.Reward != null &&
                        r.Reward.Items.Any(i => rewardItemIds.Contains(i.Id) ||
                            rewardStockIds.Contains(i.FK_ProdStockId))))
                .OrderBy(x => x.Priority)
                .ToListAsync();

            var eligibleRules = campaigns
                .SelectMany(campaign => campaign.Rules
                    .Where(rule => !rule.IsDeleted && rule.Enabled &&
                        rule.RuleType == MarketingRuleTypeEnum.AddOnPurchase &&
                        rule.Condition != null && !rule.Condition.IsDeleted &&
                        rule.Reward != null && !rule.Reward.IsDeleted)
                    .Select(rule => new
                    {
                        Campaign = campaign,
                        Rule = rule,
                        QualificationCount = GetMarketingQualificationCount(campaign, rule, baseCarts)
                    }))
                .Where(x => x.QualificationCount > 0)
                .OrderBy(x => x.Campaign.Priority)
                .ThenBy(x => x.Rule.SortOrder)
                .ToList();
            var remainingRuleAllowance = eligibleRules.ToDictionary(
                x => x.Rule.Id,
                x => x.QualificationCount * Math.Max(x.Rule.Reward!.SelectionQuantityPerQualification, 1));
            var remainingItemAllowance = eligibleRules
                .SelectMany(x => x.Rule.Reward!.Items
                    .Where(item => !item.IsDeleted && item.Enabled)
                    .Select(item => new
                    {
                        Key = (x.Rule.Id, item.Id),
                        Allowance = Math.Max(item.MaxQuantityPerOrder, 1) *
                            (x.Campaign.Repeatable ? x.QualificationCount : 1)
                    }))
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Max(y => y.Allowance));

            foreach (var additional in additionalCarts)
            {
                foreach (var eligible in eligibleRules
                    .OrderByDescending(x => x.Rule.Reward!.Items.Any(item =>
                        !item.IsDeleted && item.Enabled &&
                        additional.FK_MarketingRewardItemId.HasValue &&
                        item.Id == additional.FK_MarketingRewardItemId.Value))
                    .ThenByDescending(x => x.Rule.Reward!.Items.Any(item =>
                        !item.IsDeleted && item.Enabled &&
                        !additional.FK_MarketingRewardItemId.HasValue &&
                        item.FK_ProdStockId == additional.FK_PSid &&
                        item.OfferPrice == additional.Price)))
                {
                    var rewardItem = eligible.Rule.Reward!.Items.FirstOrDefault(item =>
                        !item.IsDeleted && item.Enabled &&
                        (additional.FK_MarketingRewardItemId.HasValue
                            ? item.Id == additional.FK_MarketingRewardItemId.Value
                            : item.FK_ProdStockId == additional.FK_PSid));
                    if (rewardItem == null)
                        continue;

                    var itemKey = (eligible.Rule.Id, rewardItem.Id);
                    var ruleRemaining = remainingRuleAllowance.GetValueOrDefault(eligible.Rule.Id);
                    var itemRemaining = remainingItemAllowance.GetValueOrDefault(itemKey);
                    if (additional.Quantity > ruleRemaining || additional.Quantity > itemRemaining)
                        continue;

                    remainingRuleAllowance[eligible.Rule.Id] = ruleRemaining - additional.Quantity;
                    remainingItemAllowance[itemKey] = itemRemaining - additional.Quantity;
                    validPrices[additional.Id] = rewardItem.OfferPrice;
                    break;
                }
            }

            return validPrices;
        }

        private ResponseMessageDto ApplyCartSpecValidation(
            Core.Models.ShoppingCart shoppingCart,
            Prod_Stock? stock = null,
            string? currentS1Title = null,
            string? currentS2Title = null)
        {
            var response = new ResponseMessageDto
            {
                Success = true
            };

            stock ??= shoppingCart.Prod_Stock;

            if (stock == null)
            {
                response.Success = false;
                response.Error = "StockNotFound";
                response.Message = "此商品目前無法購買，請移除後重新選購。";
                return response;
            }

            var s1TitleChanged = !string.Equals(
                shoppingCart.S1Title ?? "",
                currentS1Title ?? "",
                StringComparison.Ordinal);
            var s2TitleChanged = !string.Equals(
                shoppingCart.S2Title ?? "",
                currentS2Title ?? "",
                StringComparison.Ordinal);

            // 購物車提示是給使用者確認畫面可見資訊；內部規格 ID 異動但名稱相同時不提示。
            if (s1TitleChanged || s2TitleChanged)
            {
                var oldSpec = string.Join(" / ", new[] { shoppingCart.S1Title, shoppingCart.S2Title }
                    .Where(title => !string.IsNullOrWhiteSpace(title)));
                var currentSpec = string.Join(" / ", new[] { currentS1Title, currentS2Title }
                    .Where(title => !string.IsNullOrWhiteSpace(title)));

                response.Success = false;
                response.Error = "SpecTitleChanged";
                response.Message = $"商品規格：由「{oldSpec}」調整為「{currentSpec}」";
            }

            return response;
        }
        public async Task<ResponseMessageDto> Reorder(List<long> scids)
        {
            ResponseMessageDto output = new ResponseMessageDto();
            var StockAllNull = true;

            try
            {
                var oldscs = await db.ShoppingCarts.Include(e => e.Prod_Stock).ThenInclude(e => e.Prod).Where(e => scids.Contains(e.Id)).ToListAsync();
                if (oldscs.Any())
                {
                    var primaryCarts = oldscs.Where(x => !x.IsAdditional).ToList();
                    var additionalCarts = oldscs.Where(x => x.IsAdditional).ToList();

                    // 優惠商品不能再次經過一般 AddUp，否則 IsAdditional 與活動價都會遺失。
                    // 先還原主商品，再依目前仍有效的活動與購物車資格重建優惠商品。
                    foreach (var oldsc in primaryCarts)
                    {
                        if (oldsc.Prod_Stock.Prod.Status != ProdStatusEnum.售完)
                        {
                            var skipStock = oldsc.Prod_Stock.Prod.NoStockManagement;
                            if (!oldsc.Prod_Stock.Prod.RemovedFromShelves && (skipStock || oldsc.Prod_Stock.Stock > 0))
                            {
                                ShoppingCartAddUpDto newsc = new ShoppingCartAddUpDto();
                                newsc = mapper.Map<ShoppingCartAddUpDto>(oldsc);
                                newsc.Id = null;
                                if (!skipStock && newsc.Quantity > oldsc.Prod_Stock.Stock) newsc.Quantity = (int)oldsc.Prod_Stock.Stock;
                                else newsc.Quantity = oldsc.Quantity;
                                // 再買一次需保留原訂單快照，購物車才能提示商品或規格已異動。
                                var temp_response = await AddUpInternal(newsc, oldsc);
                                if (temp_response.Success) StockAllNull = false;
                                else throw new Exception(temp_response.Message);
                            }
                        }
                    }

                    if (!StockAllNull)
                    {
                        var restoredAdditionalQuantity = await RestoreReorderAdditionalItemsAsync(additionalCarts);
                        var requestedAdditionalQuantity = additionalCarts.Sum(x => x.Quantity);
                        output.Success = true;
                        output.Object = new
                        {
                            RestoredAdditionalQuantity = restoredAdditionalQuantity,
                            SkippedAdditionalQuantity = Math.Max(requestedAdditionalQuantity - restoredAdditionalQuantity, 0)
                        };
                    }
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

        private async Task<int> RestoreReorderAdditionalItemsAsync(List<Core.Models.ShoppingCart> oldAdditionalCarts)
        {
            if (!oldAdditionalCarts.Any())
                return 0;

            var uuid = await tokenAppService.GetUUID();
            var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var now = DateTime.Now;
            var rewardStockIds = oldAdditionalCarts.Select(x => x.FK_PSid).Distinct().ToList();
            var campaigns = await db.MarketingCampaigns
                .Include(x => x.Rules).ThenInclude(x => x.Condition)
                .Include(x => x.Rules).ThenInclude(x => x.ScopeItems)
                .Include(x => x.Rules).ThenInclude(x => x.Reward).ThenInclude(x => x.Items)
                    .ThenInclude(x => x.ProdStock).ThenInclude(x => x.Prod)
                .Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted &&
                    x.Status == MarketingDisplayStatusEnum.活動中 && x.StartTime <= now &&
                    (x.NeverEnd || (x.EndTime.HasValue && x.EndTime.Value >= now)) &&
                    x.Rules.Any(r => !r.IsDeleted && r.Enabled &&
                        r.RuleType == MarketingRuleTypeEnum.AddOnPurchase && r.Reward != null &&
                        r.Reward.Items.Any(i => rewardStockIds.Contains(i.FK_ProdStockId))))
                .OrderBy(x => x.Priority)
                .ToListAsync();

            if (!campaigns.Any())
                return 0;

            var currentCarts = await db.ShoppingCarts
                .Include(x => x.Prod_Stock)
                .Where(x => x.UUID == uuid && !x.IsOrder && !x.IsDeleted)
                .ToListAsync();
            var baseCarts = currentCarts.Where(x => !x.IsAdditional && x.Prod_Stock != null).ToList();
            var userId = currentCarts.Select(x => x.FK_Uid).FirstOrDefault(x => x.HasValue) ?? 0;
            var tokenId = currentCarts.Select(x => x.FK_Tid).FirstOrDefault();

            var restoredQuantity = 0;
            foreach (var oldAdditional in oldAdditionalCarts)
            {
                var candidates = campaigns
                    .SelectMany(campaign => campaign.Rules
                        .Where(rule => !rule.IsDeleted && rule.Enabled &&
                            rule.RuleType == MarketingRuleTypeEnum.AddOnPurchase &&
                            rule.Condition != null && !rule.Condition.IsDeleted &&
                            rule.Reward != null && !rule.Reward.IsDeleted &&

                            // 再買一次不自動還原「訂單層級」優惠。
                            rule.Condition.ConditionType != MarketingConditionTypeEnum.OrderAmount &&
                            rule.Condition.ConditionType != MarketingConditionTypeEnum.OrderQuantity)
                        .SelectMany(rule => rule.Reward.Items
                            .Where(item => !item.IsDeleted && item.Enabled &&
                            (oldAdditional.FK_MarketingRewardItemId.HasValue
                                ? item.Id == oldAdditional.FK_MarketingRewardItemId.Value
                                : item.FK_ProdStockId == oldAdditional.FK_PSid) &&
                                item.ProdStock != null && !item.ProdStock.IsDeleted &&
                                item.ProdStock.Prod != null && !item.ProdStock.Prod.IsDeleted &&
                                item.ProdStock.Prod.Visible && !item.ProdStock.Prod.RemovedFromShelves)
                            .Select(item => new
                            {
                                Campaign = campaign,
                                Rule = rule,
                                Item = item,
                                QualificationCount = GetMarketingQualificationCount(campaign, rule, baseCarts)
                            })))
                    .Where(x => x.QualificationCount > 0)
                    .OrderByDescending(x => x.Item.OfferPrice == oldAdditional.Price)
                    .ThenBy(x => x.Campaign.Priority)
                    .ThenBy(x => x.Rule.SortOrder)
                    .ToList();

                foreach (var candidate in candidates)
                {
                    var rewardItems = candidate.Rule.Reward!.Items
                        .Where(x => !x.IsDeleted && x.Enabled)
                        .ToList();
                    var campaignRewardItemIds = rewardItems.Select(x => x.Id).ToHashSet();
                    var campaignRewardStockIds = rewardItems.Select(x => x.FK_ProdStockId).ToHashSet();
                    var allowance = candidate.QualificationCount *
                        Math.Max(candidate.Rule.Reward.SelectionQuantityPerQualification, 1);
                    var alreadySelected = currentCarts
                        .Where(x => x.IsAdditional &&
                            (x.FK_MarketingRewardItemId.HasValue
                                ? campaignRewardItemIds.Contains(x.FK_MarketingRewardItemId.Value)
                                : campaignRewardStockIds.Contains(x.FK_PSid)))
                        .Sum(x => x.Quantity);
                    var existingItem = currentCarts
                        .Where(x => x.IsAdditional &&
                            (x.FK_MarketingRewardItemId == candidate.Item.Id ||
                             (!x.FK_MarketingRewardItemId.HasValue &&
                              x.FK_PSid == candidate.Item.FK_ProdStockId)))
                        .OrderByDescending(x => x.Price == oldAdditional.Price)
                        .ThenBy(x => x.CreationTime)
                        .FirstOrDefault();
                    var existingItemQuantity = existingItem?.Quantity ?? 0;
                    var itemLimit = Math.Max(candidate.Item.MaxQuantityPerOrder, 1) *
                        (candidate.Campaign.Repeatable ? candidate.QualificationCount : 1);
                    var availableStock = candidate.Item.ProdStock!.Prod!.NoStockManagement
                        ? int.MaxValue
                        : Math.Max((int)(candidate.Item.ProdStock.Stock ?? 0) - existingItemQuantity, 0);
                    var quantity = new[]
                    {
                        oldAdditional.Quantity,
                        Math.Max(allowance - alreadySelected, 0),
                        Math.Max(itemLimit - existingItemQuantity, 0),
                        availableStock
                    }.Min();

                    if (quantity <= 0)
                        continue;

                    if (existingItem != null)
                    {
                        existingItem.FK_MarketingRewardItemId ??= candidate.Item.Id;
                        existingItem.Quantity += quantity;
                        existingItem.LastModifierUserId = userId;
                        existingItem.LastModificationTime = now;
                    }
                    else
                    {
                        var stock = candidate.Item.ProdStock;
                        var specIds = new[] { stock.FK_S1id, stock.FK_S2id }
                            .Where(x => x.HasValue && x.Value > 0)
                            .Select(x => x!.Value)
                            .Distinct()
                            .ToList();
                        var specTitles = specIds.Any()
                            ? await db.Prod_Specs
                                .Where(x => specIds.Contains(x.Id))
                                .ToDictionaryAsync(x => x.Id, x => x.Title)
                            : new Dictionary<long, string>();
                        var cart = new Core.Models.ShoppingCart
                        {
                            FK_PSid = stock.Id,
                            FK_PriceId = null,
                            // 保留原訂單的活動價快照；目前活動價由 GetDisplay 另外計算，
                            // 讓價格異動提示在重新整理後仍然存在。
                            Price = oldAdditional.Price,
                            Bonus = 0,
                            Quantity = quantity,
                            FK_Tid = tokenId,
                            FK_Uid = userId,
                            UUID = uuid,
                            Ser_No = 500,
                            FK_S1id = stock.FK_S1id,
                            FK_S2id = stock.FK_S2id,
                            ProductId = stock.FK_Pid,
                            ProdName = stock.Prod.Title,
                            S1Title = stock.FK_S1id.HasValue && specTitles.TryGetValue(stock.FK_S1id.Value, out var s1)
                                ? s1 : null,
                            S2Title = stock.FK_S2id.HasValue && specTitles.TryGetValue(stock.FK_S2id.Value, out var s2)
                                ? s2 : null,
                            IsAdditional = true,
                            FK_MarketingRewardItemId = candidate.Item.Id,
                            CreatorUserId = userId,
                            CreationTime = now
                        };
                        db.ShoppingCarts.Add(cart);
                        currentCarts.Add(cart);
                    }

                    restoredQuantity += quantity;
                    break;
                }
            }

            await db.SaveChangesAsync();
            return restoredQuantity;
        }

        private static int GetMarketingQualificationCount(
            MarketingCampaign campaign,
            MarketingRule rule,
            List<Core.Models.ShoppingCart> baseCarts)
        {
            if (rule.Condition == null || rule.Reward == null)
                return 0;

            var scopeProductIds = rule.ScopeItems
                .Where(x => !x.IsDeleted && x.TargetType == MarketingScopeTargetTypeEnum.Product)
                .Select(x => x.TargetId)
                .Distinct()
                .ToHashSet();
            var conditionType = rule.Condition.ConditionType;

            if (conditionType == MarketingConditionTypeEnum.ScopeQuantity ||
                conditionType == MarketingConditionTypeEnum.BuySpecificProduct)
            {
                var requiredQuantity = Math.Max(rule.Condition.MinQuantity ?? 1, 1);
                var quantity = baseCarts
                    .Where(x => x.Prod_Stock != null && scopeProductIds.Contains(x.Prod_Stock.FK_Pid))
                    .Sum(x => x.Quantity);
                return campaign.Repeatable
                    ? quantity / requiredQuantity
                    : quantity >= requiredQuantity ? 1 : 0;
            }

            if (conditionType == MarketingConditionTypeEnum.OrderAmount ||
                conditionType == MarketingConditionTypeEnum.ScopeAmount)
            {
                var requiredAmount = Math.Max(rule.Condition.MinAmount ?? 0, 0);
                if (requiredAmount <= 0)
                    return 0;

                var amount = baseCarts
                    .Where(x => x.Prod_Stock != null &&
                        (conditionType != MarketingConditionTypeEnum.ScopeAmount ||
                         scopeProductIds.Contains(x.Prod_Stock.FK_Pid)))
                    .Sum(x => x.Price * x.Quantity);
                return campaign.Repeatable
                    ? (int)Math.Floor(amount / requiredAmount)
                    : amount >= requiredAmount ? 1 : 0;
            }

            return 0;
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
                var uuid = await tokenAppService.GetUUID();
                var result = await db.ShoppingCarts
                    .Where(e => e.Id == id && e.UUID == uuid && !e.IsOrder && !e.IsDeleted)
                    .FirstOrDefaultAsync();

                if (result != null)
                {
                    var db_ps = await db.Prod_Stocks.Where(e => e.Id == result.FK_PSid).FirstOrDefaultAsync();
                    if (db_ps != null)
                    {
                        var now = DateTime.Now;
                        result.IsDeleted = true;
                        result.DeletionTime = now;
                        result.DeleterUserId = result.CreatorUserId;

                        var removedAdditionalIds = result.IsAdditional
                            ? new List<long>()
                            : await RemoveInvalidAdditionalItemsAsync(
                                uuid,
                                result.Id,
                                db_ps.FK_Pid,
                                result.CreatorUserId,
                                now);

                        await db.SaveChangesAsync();
                        output.Success = true;
                        output.Message = removedAdditionalIds.Any()
                            ? $"主要商品已移除，並一併移除 {removedAdditionalIds.Count} 項已失效的優惠商品。"
                            : "商品已移除。";
                        output.Object = new { RemovedCartIds = removedAdditionalIds };
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

        private async Task<List<long>> RemoveInvalidAdditionalItemsAsync(
            Guid uuid,
            long removedPrimaryCartId,
            long removedProductId,
            long userId,
            DateTime now)
        {
            var carts = await db.ShoppingCarts
                .Include(x => x.Prod_Stock)
                .Where(x => x.UUID == uuid && !x.IsOrder && !x.IsDeleted && x.Id != removedPrimaryCartId)
                .OrderBy(x => x.CreationTime)
                .ToListAsync();
            var baseCarts = carts.Where(x => !x.IsAdditional && x.Prod_Stock != null).ToList();
            var additionalCarts = carts.Where(x => x.IsAdditional).ToList();
            if (!additionalCarts.Any())
                return new List<long>();

            var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var rewardStockIds = additionalCarts.Select(x => x.FK_PSid).Distinct().ToList();
            // 停止或過期的活動仍要拿來辨識原本的主從關係，因此這裡先載入所有關聯，
            // 再另外判斷哪些規則目前仍可支撐購物車內的優惠商品。
            var campaigns = await db.MarketingCampaigns
                .IgnoreQueryFilters()
                .Include(x => x.Rules).ThenInclude(x => x.Condition)
                .Include(x => x.Rules).ThenInclude(x => x.ScopeItems)
                .Include(x => x.Rules).ThenInclude(x => x.Reward).ThenInclude(x => x.Items)
                .Where(x => x.FK_WebsiteId == websiteId &&
                    x.Rules.Any(r => !r.IsDeleted && r.Enabled &&
                        r.RuleType == MarketingRuleTypeEnum.AddOnPurchase && r.Reward != null &&
                        r.Reward.Items.Any(i => rewardStockIds.Contains(i.FK_ProdStockId))))
                .OrderBy(x => x.Priority)
                .ToListAsync();

            var relatedRules = campaigns
                .SelectMany(campaign => campaign.Rules
                    .Where(rule => rule.RuleType == MarketingRuleTypeEnum.AddOnPurchase &&
                        rule.Condition != null && rule.Reward != null)
                    .Select(rule => new
                    {
                        Campaign = campaign,
                        Rule = rule
                    }))
                .ToList();

            bool IsAffectedByRemovedProduct(MarketingRule rule)
            {
                if (rule.Condition == null)
                    return false;

                if (rule.Condition.ConditionType == MarketingConditionTypeEnum.OrderAmount)
                    return true;

                return rule.ScopeItems.Any(x =>
                    !x.IsDeleted &&
                    x.TargetType == MarketingScopeTargetTypeEnum.Product &&
                    x.TargetId == removedProductId);
            }

            var eligibleRules = relatedRules
                .Where(x => IsAffectedByRemovedProduct(x.Rule) &&
                    !x.Campaign.IsDeleted &&
                    x.Campaign.Status == MarketingDisplayStatusEnum.活動中 &&
                    x.Campaign.StartTime <= now &&
                    (x.Campaign.NeverEnd ||
                     (x.Campaign.EndTime.HasValue && x.Campaign.EndTime.Value >= now)) &&
                    !x.Rule.IsDeleted && x.Rule.Enabled &&
                    x.Rule.Condition != null && !x.Rule.Condition.IsDeleted &&
                    x.Rule.Reward != null && !x.Rule.Reward.IsDeleted)
                .Select(x => new
                {
                    x.Campaign,
                    x.Rule,
                    QualificationCount = GetMarketingQualificationCount(x.Campaign, x.Rule, baseCarts)
                })
                .Where(x => x.QualificationCount > 0)
                .ToList();
            var remainingRuleAllowance = eligibleRules.ToDictionary(
                x => x.Rule.Id,
                x => x.QualificationCount * Math.Max(x.Rule.Reward!.SelectionQuantityPerQualification, 1));
            var remainingItemAllowance = eligibleRules
                .SelectMany(x => x.Rule.Reward!.Items
                    .Where(item => !item.IsDeleted && item.Enabled)
                    .Select(item => new
                    {
                        Key = (x.Rule.Id, item.Id),
                        Allowance = Math.Max(item.MaxQuantityPerOrder, 1) *
                            (x.Campaign.Repeatable ? x.QualificationCount : 1)
                    }))
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Max(y => y.Allowance));
            var removedIds = new List<long>();

            foreach (var additional in additionalCarts)
            {
                var matchingRelatedRules = relatedRules
                    .Where(x => x.Rule.Reward!.Items.Any(item =>
                        additional.FK_MarketingRewardItemId.HasValue
                            ? item.Id == additional.FK_MarketingRewardItemId.Value
                            : item.FK_ProdStockId == additional.FK_PSid))
                    .ToList();
                var affectedRuleIds = matchingRelatedRules
                    .Where(x => IsAffectedByRemovedProduct(x.Rule))
                    .Select(x => x.Rule.Id)
                    .ToHashSet();

                // 找不到任何來源規則代表這是孤立的優惠商品；若規則與被刪除商品無關，
                // 則保留給它原本的其他活動處理。
                if (matchingRelatedRules.Any() && !affectedRuleIds.Any())
                    continue;

                var matched = false;
                foreach (var eligible in eligibleRules
                    .Where(x => affectedRuleIds.Contains(x.Rule.Id))
                    .OrderBy(x => x.Campaign.Priority)
                    .ThenBy(x => x.Rule.SortOrder))
                {
                    var rewardItem = eligible.Rule.Reward!.Items.FirstOrDefault(item =>
                        !item.IsDeleted && item.Enabled &&
                        item.FK_ProdStockId == additional.FK_PSid);
                    if (rewardItem == null)
                        continue;

                    var itemKey = (eligible.Rule.Id, rewardItem.Id);
                    var ruleRemaining = remainingRuleAllowance.GetValueOrDefault(eligible.Rule.Id);
                    var itemRemaining = remainingItemAllowance.GetValueOrDefault(itemKey);
                    if (additional.Quantity > ruleRemaining || additional.Quantity > itemRemaining)
                        continue;

                    remainingRuleAllowance[eligible.Rule.Id] = ruleRemaining - additional.Quantity;
                    remainingItemAllowance[itemKey] = itemRemaining - additional.Quantity;
                    matched = true;
                    break;
                }

                if (matched)
                    continue;

                additional.IsDeleted = true;
                additional.DeletionTime = now;
                additional.DeleterUserId = userId;
                removedIds.Add(additional.Id);
            }

            return removedIds;
        }
    }
}

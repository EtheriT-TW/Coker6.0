using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.Role;
using EtheriT.Coker.Application.Shared.i18n;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.Product
{
    public class ProductDisplayPriceService : IProductDisplayPriceService
    {
        private readonly CokerDbContext db;
        private readonly IBonusManagementAppService bonusManagementAppService;

        public ProductDisplayPriceService(CokerDbContext db, IBonusManagementAppService bonusManagementAppService)
        {
            this.db = db;
            this.bonusManagementAppService = bonusManagementAppService;
        }

        public async Task<Dictionary<long, DirectoryPriceResultDto>> GetDirectoryPriceMapAsync(
            List<long> productIds,
            FrontRoleContextDto roleContext,
            bool orderLowToHigh
        )
        {
            var result = new Dictionary<long, DirectoryPriceResultDto>();

            try
            {
                if (productIds == null || productIds.Count == 0)
                    return result;

                var websiteId = roleContext.WebsiteId;
                var roleIndex = roleContext.RoleIndex;
                var visibleRoleIds = roleContext.VisibleRoleIds;
                var roleLevels = roleContext.RoleLevels
                    .Select(e => (e.Id, e.Name))
                    .ToList();
                var bonusSetting = await bonusManagementAppService.GetBonusSettingForEdit();
                var bonusEnabled = bonusSetting?.BonusEnabled == true;

                var stocks = await db.Prod_Stocks
                    .Where(e => productIds.Contains(e.FK_Pid) && !e.IsDeleted)
                    .ToListAsync();

                if (!stocks.Any())
                    return result;

                var stockIds = stocks.Select(e => e.Id).ToList();

                var prices = await db.Prod_Prices
                    .Where(e => stockIds.Contains(e.FK_PSId) && !e.IsDeleted)
                    .ToListAsync();

                var stocksByProduct = stocks
                    .GroupBy(e => e.FK_Pid)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var pricesByStock = prices
                    .GroupBy(e => e.FK_PSId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var productId in productIds)
                {
                    if (!stocksByProduct.TryGetValue(productId, out var productStocks) || !productStocks.Any())
                        continue;

                    var productStockIds = productStocks.Select(e => e.Id).ToList();

                    var hasBonusPrice =
                        bonusEnabled &&
                        prices.Any(e =>
                            productStockIds.Contains(e.FK_PSId) &&
                            (e.Bonus ?? 0) > 0 &&
                            e.FK_RId > 1);

                    var candidates = new List<(
                        Prod_Stock Stock,
                        Prod_Price Price,
                        string? CurrentRoleName,
                        bool IsMemberPrice,
                        int Rank,
                        decimal SortValue,
                        decimal? OriPrice,
                        string? BaseRoleName
                    )>();

                    foreach (var stock in productStocks.Where(e => !e.IsTimePrice))
                    {
                        if (!pricesByStock.TryGetValue(stock.Id, out var stockPrices) || !stockPrices.Any())
                            continue;

                        var selected = SelectDisplayPriceForStock(
                            stockPrices,
                            visibleRoleIds,
                            roleLevels,
                            roleIndex,
                            orderLowToHigh,
                            bonusEnabled);

                        if (selected == null || selected.Price == null)
                            continue;

                        var basePriceInfo = GetBasePriceInfo(stockPrices, roleLevels);

                        candidates.Add((
                            Stock: stock,
                            Price: selected.Price,
                            CurrentRoleName: selected.CurrentRoleName,
                            IsMemberPrice: selected.IsMemberPrice,
                            Rank: selected.Rank,
                            SortValue: selected.SortValue,
                            OriPrice: basePriceInfo.Price,
                            BaseRoleName: basePriceInfo.BaseRoleName
                        ));
                    }

                    if (candidates.Any())
                    {
                        var chosen = orderLowToHigh
                            ? candidates
                                .OrderBy(e => e.SortValue)
                                .ThenBy(e => e.Price.Bonus ?? 0)
                                .ThenBy(e => e.Rank)
                                .ThenBy(e => e.Stock.Ser_No)
                                .ThenBy(e => e.Stock.Id)
                                .First()
                            : candidates
                                .OrderByDescending(e => e.SortValue)
                                .ThenBy(e => e.Price.Bonus ?? 0)
                                .ThenBy(e => e.Rank)
                                .ThenBy(e => e.Stock.Ser_No)
                                .ThenBy(e => e.Stock.Id)
                                .First();

                        var priceValue = chosen.Price.Price ?? 0;
                        var bonusValue = chosen.Price.Bonus ?? 0;

                        result[productId] = new DirectoryPriceResultDto
                        {
                            ProductId = productId,
                            Price = priceValue > 0 ? priceValue.ToString("N0") : null,
                            Bonus = bonusValue > 0 ? bonusValue.ToString("N0") : null,
                            OriPrice = chosen.OriPrice.HasValue && chosen.OriPrice.Value > 0
                                ? chosen.OriPrice.Value.ToString("N0")
                                : null,
                            SuggestPrice = chosen.Stock.Price > 0
                                ? chosen.Stock.Price.ToString("N0")
                                : null,
                            IsTimePrice = false,
                            IsMemberPrice = chosen.IsMemberPrice,
                            PriceDisplayText = null,
                            BaseRoleName = chosen.BaseRoleName,
                            CurrentRoleName = chosen.CurrentRoleName,
                            HasBonusPrice = hasBonusPrice
                        };

                        continue;
                    }

                    var timeStock = productStocks.FirstOrDefault(e => e.IsTimePrice);
                    if (timeStock != null)
                    {
                        result[productId] = new DirectoryPriceResultDto
                        {
                            ProductId = productId,
                            Price = null,
                            Bonus = null,
                            OriPrice = null,
                            SuggestPrice = null,
                            IsTimePrice = true,
                            IsMemberPrice = false,
                            PriceDisplayText = L.get("MarketPrice"),
                            HasBonusPrice = hasBonusPrice
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("-------------錯誤訊息查看-------------");
                Console.WriteLine($"Product=>ProductDisplayPriceService.GetDirectoryPriceMapAsync：{ex.Message}");
            }

            return result;
        }
        public async Task<DirectoryPriceResultDto?> GetProductDisplayPriceAsync(
            long productId,
            FrontRoleContextDto roleContext,
            bool orderLowToHigh
        )
        {
            var map = await GetDirectoryPriceMapAsync(
                new List<long> { productId },
                roleContext,
                orderLowToHigh);

            return map.TryGetValue(productId, out var result) ? result : null;
        }
        public async Task<List<ProductPriceDto>> GetDisplayPricesByStockAsync(
            List<long> stockIds,
            FrontRoleContextDto roleContext
        )
        {
            var output = new List<ProductPriceDto>();

            try
            {
                if (stockIds == null || stockIds.Count == 0)
                    return output;

                var bonusSetting = await bonusManagementAppService.GetBonusSettingForEdit();
                var bonusEnabled = bonusSetting?.BonusEnabled == true;

                var roleLevels = roleContext.RoleLevels
                    .Select(e => (e.Id, e.Name))
                    .ToList();

                var visibleRoleIds = roleContext.VisibleRoleIds;
                var roleIndex = roleContext.RoleIndex;

                string GetRoleName(long roleId)
                {
                    if (roleId == 1 || roleId == 0)
                        return "非會員";

                    return roleLevels
                        .Where(e => e.Id == roleId)
                        .Select(e => e.Name)
                        .FirstOrDefault() ?? "";
                }

                var baseRoleName = GetRoleName(1);

                ProductPriceDto ToPriceDto(Prod_Price entity, decimal? oriPrice)
                {
                    return new ProductPriceDto
                    {
                        Id = entity.Id,
                        FK_PSId = entity.FK_PSId,
                        FK_RId = entity.FK_RId,
                        Price = entity.Price,
                        Bonus = entity.Bonus ?? 0,
                        OriPrice = oriPrice,
                        RoleName = GetRoleName(entity.FK_RId),
                        BaseRoleName = baseRoleName
                    };
                }

                var allPrices = await db.Prod_Prices
                    .Where(e => stockIds.Contains(e.FK_PSId) && !e.IsDeleted)
                    .ToListAsync();

                var pricesByStock = allPrices
                    .GroupBy(e => e.FK_PSId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var stockId in stockIds)
                {
                    if (!pricesByStock.TryGetValue(stockId, out var stockPrices) || !stockPrices.Any())
                        continue;

                    var guestCashEntity = stockPrices
                        .Where(e => (e.Bonus ?? 0) == 0 && (e.FK_RId == 1 || e.FK_RId == 0))
                        .OrderBy(e => e.FK_RId == 1 ? 0 : 1)
                        .ThenBy(e => e.Price ?? 0)
                        .ThenBy(e => e.Id)
                        .FirstOrDefault();

                    var guestOriPrice = guestCashEntity?.Price;

                    void AddIfNotExists(Prod_Price entity, decimal? oriPrice)
                    {
                        var exists = output.Any(e =>
                            e.FK_PSId == entity.FK_PSId &&
                            e.Id == entity.Id);

                        if (exists)
                            return;

                        output.Add(ToPriceDto(entity, oriPrice));
                    }

                    bool IsSamePlanKind(Prod_Price a, Prod_Price b)
                    {
                        var aBonus = a.Bonus ?? 0;
                        var bBonus = b.Bonus ?? 0;

                        if (aBonus == 0 && bBonus == 0)
                            return true;

                        if (aBonus > 0 && bBonus > 0)
                            return true;

                        return false;
                    }

                    bool CanReplace(Prod_Price currentPlan, Prod_Price lowerPlan)
                    {
                        if (!IsSamePlanKind(currentPlan, lowerPlan))
                            return false;

                        var currentPrice = currentPlan.Price ?? 0;
                        var currentBonus = currentPlan.Bonus ?? 0;
                        var lowerPrice = lowerPlan.Price ?? 0;
                        var lowerBonus = lowerPlan.Bonus ?? 0;

                        return currentPrice <= lowerPrice && currentBonus <= lowerBonus;
                    }

                    // roleIndex 是目前角色在 roleLevels 的位置。
                    // 例如：
                    // 非會員 index = 0
                    // 一般會員 index = 1
                    // 更高會員 index = 2...
                    var safeRoleIndex = roleIndex;

                    if (safeRoleIndex < 0 || safeRoleIndex >= roleLevels.Count)
                    {
                        safeRoleIndex = roleLevels.FindIndex(e => e.Id == 1 || e.Id == 0);
                        if (safeRoleIndex < 0)
                            safeRoleIndex = 0;
                    }

                    var currentRole = roleLevels[safeRoleIndex];

                    // 目前角色自己的方案全部顯示。
                    // 這裡不再替客戶判斷「紅利價現金部分比較高就隱藏」。
                    var currentRolePlans = stockPrices
                        .Where(e => e.FK_RId == currentRole.Id)
                        .Where(e => bonusEnabled || (e.Bonus ?? 0) == 0)
                        .OrderBy(e => e.Bonus ?? 0)
                        .ThenBy(e => e.Price ?? 0)
                        .ThenBy(e => e.Id)
                        .ToList();

                    foreach (var plan in currentRolePlans)
                    {
                        AddIfNotExists(
                            plan,
                            guestOriPrice ?? plan.Price
                        );
                    }

                    // 低階角色方案只在「目前角色沒有完整平替」時才補。
                    // 平替判斷：現金與紅利兩個維度都 <= 低階方案，才視為可取代。
                    var lowerRoleIds = roleLevels
                        .Take(safeRoleIndex)
                        .Select(e => e.Id)
                        .ToList();

                    var lowerRolePlans = stockPrices
                        .Where(e => lowerRoleIds.Contains(e.FK_RId))
                        .Where(e => bonusEnabled || (e.Bonus ?? 0) == 0)
                        .OrderBy(e => e.FK_RId == 1 || e.FK_RId == 0 ? 0 : 1)
                        .ThenBy(e => e.Bonus ?? 0)
                        .ThenBy(e => e.Price ?? 0)
                        .ThenBy(e => e.Id)
                        .ToList();

                    foreach (var lowerPlan in lowerRolePlans)
                    {
                        var hasReplacement = currentRolePlans.Any(currentPlan =>
                            CanReplace(currentPlan, lowerPlan));

                        if (hasReplacement)
                            continue;

                        AddIfNotExists(
                            lowerPlan,
                            guestOriPrice ?? lowerPlan.Price
                        );
                    }

                    // 非會員情境：
                    // 非會員自己的價格通常只有現金價。
                    // 但商品內頁價格列表需要展示一筆會員紅利價，讓前端顯示，但不能買由前端/加入購物車流程判斷。
                    if (bonusEnabled && (currentRole.Id == 1 || currentRole.Id == 0))
                    {
                        var alreadyHasBonus = output.Any(e =>
                            e.FK_PSId == stockId && e.Bonus > 0);

                        if (!alreadyHasBonus)
                        {
                            var displayBonus = stockPrices
                                .Where(e => (e.Bonus ?? 0) > 0)
                                .OrderBy(e => e.FK_RId == 1 || e.FK_RId == 0 ? 1 : 0)
                                .ThenBy(e => e.Price ?? 0)
                                .ThenBy(e => e.Bonus ?? 0)
                                .ThenBy(e => e.Id)
                                .FirstOrDefault();

                            if (displayBonus != null)
                            {
                                AddIfNotExists(
                                    displayBonus,
                                    guestOriPrice ?? displayBonus.Price
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("-------------錯誤訊息查看-------------");
                Console.WriteLine($"Product=>ProductDisplayPriceService.GetDisplayPricesByStockAsync：{ex.Message}");
            }

            return output;
        }

        private sealed class DisplayPriceSelection
        {
            public Prod_Price? Price { get; set; }
            public string? CurrentRoleName { get; set; }
            public bool IsMemberPrice { get; set; }
            public int Rank { get; set; }
            public decimal SortValue { get; set; }
            public bool IsBonusPrice { get; set; }
        }

        private DisplayPriceSelection? SelectDisplayPriceForStock(
            List<Prod_Price> stockPrices,
            List<long> visibleRoleIds,
            List<(long Id, string Name)> roleLevels,
            int roleIndex,
            bool orderLowToHigh,
            bool bonusEnabled
        )
        {
            if (stockPrices == null || stockPrices.Count == 0)
                return null;

            var candidates = new List<DisplayPriceSelection>();

            for (int index = roleIndex; index >= 0; index--)
            {
                var currentRole = roleLevels[index];

                var cash = stockPrices
                    .Where(e => (e.Bonus ?? 0) == 0 && e.FK_RId == currentRole.Id)
                    .OrderBy(e => e.Price ?? 0)
                    .ThenBy(e => e.Id)
                    .FirstOrDefault();

                if (cash != null)
                {
                    candidates.Add(new DisplayPriceSelection
                    {
                        Price = cash,
                        CurrentRoleName = currentRole.Name,
                        IsMemberPrice = cash.FK_RId > 1,
                        Rank = 0,
                        SortValue = cash.Price ?? 0,
                        IsBonusPrice = false
                    });

                    break;
                }
            }

            if (bonusEnabled)
            {
                var bonusMatched = stockPrices
                    .Where(e => (e.Bonus ?? 0) > 0 && visibleRoleIds.Contains(e.FK_RId))
                    .OrderBy(e => e.Price ?? 0)
                    .ThenBy(e => e.Bonus ?? 0)
                    .ThenBy(e => e.Id)
                    .FirstOrDefault();

                if (bonusMatched != null)
                {
                    var roleName = roleLevels
                        .Where(e => e.Id == bonusMatched.FK_RId)
                        .Select(e => e.Name)
                        .FirstOrDefault();

                    candidates.Add(new DisplayPriceSelection
                    {
                        Price = bonusMatched,
                        CurrentRoleName = roleName ?? "",
                        IsMemberPrice = bonusMatched.FK_RId > 1,
                        Rank = 1,
                        SortValue = bonusMatched.Price ?? 0,
                        IsBonusPrice = true
                    });
                }
            }

            if (!candidates.Any())
            {
                var fallbackCash = stockPrices
                    .Where(e => (e.Bonus ?? 0) == 0)
                    .OrderBy(e => (e.FK_RId == 1 || e.FK_RId == 0) ? 0 : 1)
                    .ThenBy(e => e.FK_RId)
                    .ThenBy(e => e.Price ?? 0)
                    .ThenBy(e => e.Id)
                    .FirstOrDefault();

                if (fallbackCash != null)
                {
                    var roleName = roleLevels
                        .Where(e => e.Id == fallbackCash.FK_RId)
                        .Select(e => e.Name)
                        .FirstOrDefault();

                    return new DisplayPriceSelection
                    {
                        Price = fallbackCash,
                        CurrentRoleName = roleName ?? "",
                        IsMemberPrice = fallbackCash.FK_RId > 1,
                        Rank = 2,
                        SortValue = fallbackCash.Price ?? 0,
                        IsBonusPrice = false
                    };
                }

                if (!bonusEnabled)
                    return null;

                var fallbackBonus = stockPrices
                    .Where(e => (e.Bonus ?? 0) > 0)
                    .OrderBy(e => (e.FK_RId == 1 || e.FK_RId == 0) ? 0 : 1)
                    .ThenBy(e => e.FK_RId)
                    .ThenBy(e => e.Price ?? 0)
                    .ThenBy(e => e.Bonus ?? 0)
                    .ThenBy(e => e.Id)
                    .FirstOrDefault();

                if (fallbackBonus != null)
                {
                    var roleName = roleLevels
                        .Where(e => e.Id == fallbackBonus.FK_RId)
                        .Select(e => e.Name)
                        .FirstOrDefault();

                    return new DisplayPriceSelection
                    {
                        Price = fallbackBonus,
                        CurrentRoleName = roleName ?? "",
                        IsMemberPrice = fallbackBonus.FK_RId > 1,
                        Rank = 3,
                        SortValue = fallbackBonus.Price ?? 0,
                        IsBonusPrice = true
                    };
                }

                return null;
            }

            return orderLowToHigh
                ? candidates
                    .OrderBy(e => e.SortValue)
                    .ThenBy(e => e.Price?.Bonus ?? 0)
                    .ThenBy(e => e.Rank)
                    .First()
                : candidates
                    .OrderByDescending(e => e.SortValue)
                    .ThenBy(e => e.Price?.Bonus ?? 0)
                    .ThenBy(e => e.Rank)
                    .First();
        }

        private (decimal? Price, string? BaseRoleName) GetBasePriceInfo(
            List<Prod_Price> stockPrices,
            List<(long Id, string Name)> roleLevels
        )
        {
            if (stockPrices == null || stockPrices.Count == 0)
                return (null, null);

            var guestCash = stockPrices
                .Where(e =>
                    (e.FK_RId == 1 || e.FK_RId == 0) &&
                    (e.Bonus ?? 0) == 0 &&
                    (e.Price ?? 0) > 0)
                .OrderBy(e => e.FK_RId == 1 ? 0 : 1)
                .ThenBy(e => e.Price ?? 0)
                .ThenBy(e => e.Id)
                .FirstOrDefault();

            if (guestCash != null)
                return (guestCash.Price, "非會員");

            var firstFrontRole = roleLevels.Skip(1).FirstOrDefault();
            if (firstFrontRole.Id == 0)
                return (null, null);

            var firstRoleCash = stockPrices
                .Where(e =>
                    e.FK_RId == firstFrontRole.Id &&
                    (e.Bonus ?? 0) == 0 &&
                    (e.Price ?? 0) > 0)
                .OrderBy(e => e.Price ?? 0)
                .ThenBy(e => e.Id)
                .FirstOrDefault();

            if (firstRoleCash != null)
                return (firstRoleCash.Price, firstFrontRole.Name);

            return (null, null);
        }
    }
}
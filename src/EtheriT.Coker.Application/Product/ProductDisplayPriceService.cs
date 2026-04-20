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

        public ProductDisplayPriceService(CokerDbContext db)
        {
            this.db = db;
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
                            orderLowToHigh);

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
                            CurrentRoleName = chosen.CurrentRoleName
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
                            PriceDisplayText = L.get("MarketPrice")
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

                var roleLevels = roleContext.RoleLevels
                    .Select(e => (e.Id, e.Name))
                    .ToList();

                var visibleRoleIds = roleContext.VisibleRoleIds;
                var roleIndex = roleContext.RoleIndex;

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

                    // 先選現金價
                    ProductPriceDto? selectedCash = null;
                    ProductPriceDto? guestCash = null;

                    var guestCashEntity = stockPrices
                        .Where(e => (e.Bonus ?? 0) == 0 && (e.FK_RId == 1 || e.FK_RId == 0))
                        .OrderBy(e => e.FK_RId == 1 ? 0 : 1)
                        .ThenBy(e => e.Price ?? 0)
                        .ThenBy(e => e.Id)
                        .FirstOrDefault();

                    if (guestCashEntity != null)
                    {
                        guestCash = new ProductPriceDto
                        {
                            Id = guestCashEntity.Id,
                            FK_PSId = guestCashEntity.FK_PSId,
                            FK_RId = guestCashEntity.FK_RId,
                            Price = guestCashEntity.Price,
                            Bonus = guestCashEntity.Bonus ?? 0,
                            OriPrice = guestCashEntity.Price
                        };
                    }

                    for (int index = roleIndex; index >= 0; index--)
                    {
                        var currentRole = roleLevels[index];

                        var cashEntity = stockPrices
                            .Where(e => (e.Bonus ?? 0) == 0 && e.FK_RId == currentRole.Id)
                            .OrderBy(e => e.Price ?? 0)
                            .ThenBy(e => e.Id)
                            .FirstOrDefault();

                        if (cashEntity != null)
                        {
                            selectedCash = new ProductPriceDto
                            {
                                Id = cashEntity.Id,
                                FK_PSId = cashEntity.FK_PSId,
                                FK_RId = cashEntity.FK_RId,
                                Price = cashEntity.Price,
                                Bonus = cashEntity.Bonus ?? 0,
                                OriPrice = guestCash?.OriPrice ?? guestCashEntity?.Price ?? cashEntity.Price
                            };
                            break;
                        }
                    }

                    if (selectedCash == null)
                    {
                        var fallbackCashEntity = stockPrices
                            .Where(e => (e.Bonus ?? 0) == 0)
                            .OrderBy(e => (e.FK_RId == 1 || e.FK_RId == 0) ? 0 : 1)
                            .ThenBy(e => e.FK_RId)
                            .ThenBy(e => e.Price ?? 0)
                            .ThenBy(e => e.Id)
                            .FirstOrDefault();

                        if (fallbackCashEntity != null)
                        {
                            selectedCash = new ProductPriceDto
                            {
                                Id = fallbackCashEntity.Id,
                                FK_PSId = fallbackCashEntity.FK_PSId,
                                FK_RId = fallbackCashEntity.FK_RId,
                                Price = fallbackCashEntity.Price,
                                Bonus = fallbackCashEntity.Bonus ?? 0,
                                OriPrice = guestCash?.OriPrice ?? guestCashEntity?.Price ?? fallbackCashEntity.Price
                            };
                        }
                    }

                    if (selectedCash != null)
                    {
                        output.Add(selectedCash);
                    }

                    // 再選紅利價（沿用你原本可見角色範圍概念）
                    var bonusEntity = stockPrices
                        .Where(e => (e.Bonus ?? 0) > 0 && visibleRoleIds.Contains(e.FK_RId))
                        .OrderBy(e => e.Price ?? 0)
                        .ThenBy(e => e.Bonus ?? 0)
                        .ThenBy(e => e.Id)
                        .FirstOrDefault();

                    if (bonusEntity != null)
                    {
                        var cashPrice = output
                            .Where(e => e.FK_PSId == stockId && e.Bonus == 0)
                            .FirstOrDefault();

                        if (cashPrice == null || (cashPrice.Price ?? 0) > (bonusEntity.Price ?? 0))
                        {
                            output.Add(new ProductPriceDto
                            {
                                Id = bonusEntity.Id,
                                FK_PSId = bonusEntity.FK_PSId,
                                FK_RId = bonusEntity.FK_RId,
                                Price = bonusEntity.Price,
                                Bonus = bonusEntity.Bonus ?? 0,
                                OriPrice = guestCash?.OriPrice ?? guestCashEntity?.Price
                            });
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
            bool orderLowToHigh
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
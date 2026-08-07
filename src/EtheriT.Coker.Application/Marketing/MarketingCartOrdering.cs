using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.Marketing
{
    internal sealed class MarketingRewardOrderingInfo
    {
        public MarketingConditionTypeEnum ConditionType { get; init; }
        public HashSet<long> ScopeProductIds { get; init; } = new();
    }

    /// <summary>
    /// 保留一般商品原本的順序，將指定商品型優惠排在最後一個符合範圍的主商品後方，
    /// 訂單滿額／滿件優惠及無法辨識來源的舊資料則統一排在最後。
    /// </summary>
    internal static class MarketingCartOrdering
    {
        public static async Task<Dictionary<long, MarketingRewardOrderingInfo>> LoadRewardInfosAsync(
            CokerDbContext db,
            IEnumerable<long?> rewardItemIds)
        {
            var ids = rewardItemIds.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            if (!ids.Any())
                return new Dictionary<long, MarketingRewardOrderingInfo>();

            var rewardItems = await db.MarketingRewardItems
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(x => x.MarketingReward)
                    .ThenInclude(x => x.MarketingRule)
                        .ThenInclude(x => x.Condition)
                .Include(x => x.MarketingReward)
                    .ThenInclude(x => x.MarketingRule)
                        .ThenInclude(x => x.ScopeItems)
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            return rewardItems.ToDictionary(
                x => x.Id,
                x => new MarketingRewardOrderingInfo
                {
                    ConditionType = x.MarketingReward.MarketingRule.Condition.ConditionType,
                    ScopeProductIds = x.MarketingReward.MarketingRule.ScopeItems
                        .Where(scope => !scope.IsDeleted && scope.TargetType == MarketingScopeTargetTypeEnum.Product)
                        .Select(scope => scope.TargetId)
                        .ToHashSet()
                });
        }

        public static List<T> Sort<T>(
            IReadOnlyList<T> items,
            Func<T, bool> isAdditional,
            Func<T, long> productId,
            Func<T, long?> rewardItemId,
            IReadOnlyDictionary<long, MarketingRewardOrderingInfo> rewardInfos)
        {
            var primaryItems = items
                .Select((item, index) => new { Item = item, Index = index })
                .Where(x => !isAdditional(x.Item))
                .ToList();
            var anchored = new Dictionary<int, List<(T Item, int Index)>>();
            var trailing = new List<(T Item, int Index)>();

            foreach (var additional in items
                .Select((item, index) => new { Item = item, Index = index })
                .Where(x => isAdditional(x.Item)))
            {
                var sourceId = rewardItemId(additional.Item);
                if (!sourceId.HasValue || !rewardInfos.TryGetValue(sourceId.Value, out var info) ||
                    info.ConditionType == MarketingConditionTypeEnum.OrderAmount ||
                    info.ConditionType == MarketingConditionTypeEnum.OrderQuantity)
                {
                    trailing.Add((additional.Item, additional.Index));
                    continue;
                }

                var anchor = primaryItems
                    .Select((primary, index) => new { PrimaryIndex = index, ProductId = productId(primary.Item) })
                    .Where(x => info.ScopeProductIds.Contains(x.ProductId))
                    .Select(x => (int?)x.PrimaryIndex)
                    .LastOrDefault();

                if (!anchor.HasValue)
                {
                    trailing.Add((additional.Item, additional.Index));
                    continue;
                }

                if (!anchored.TryGetValue(anchor.Value, out var group))
                {
                    group = new List<(T Item, int Index)>();
                    anchored[anchor.Value] = group;
                }
                group.Add((additional.Item, additional.Index));
            }

            var result = new List<T>(items.Count);
            for (var index = 0; index < primaryItems.Count; index++)
            {
                result.Add(primaryItems[index].Item);
                if (anchored.TryGetValue(index, out var group))
                    result.AddRange(group.OrderBy(x => x.Index).Select(x => x.Item));
            }
            result.AddRange(trailing.OrderBy(x => x.Index).Select(x => x.Item));
            return result;
        }
    }
}

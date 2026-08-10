using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;
using EtheriT.Coker.Application.Shared.Dto.Marketing;
using EtheriT.Coker.Application.Shared.Marketing;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EtheriT.Coker.Application.Marketing
{
    public class CheckoutDiscountService : ICheckoutDiscountService
    {
        private sealed class DiscountCandidate
        {
            public CheckoutDiscountAppliedDto Discount { get; init; } = new();
            public bool CanStack { get; init; }
            public int Priority { get; init; }
            public int Sequence { get; init; }
            public decimal MinAmount { get; init; }
            public decimal DiscountPercent { get; init; }
            public decimal MaxDiscountAmount { get; init; }
        }

        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IConfiguration configuration;

        public CheckoutDiscountService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IConfiguration configuration)
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.configuration = configuration;
        }

        public async Task<CheckoutDiscountResultDto> CalculateAsync(CheckoutDiscountInputDto input)
        {
            try
            {
                var websiteId = await loginUserData.GetCommonWebsiteId();

                var now = DateTime.Now;

                var result = new CheckoutDiscountResultDto();

                if (input == null || input.Items == null || !input.Items.Any())
                    return result;

                var productSubtotal = input.Items.Sum(x => x.Subtotal);

                if (productSubtotal <= 0)
                    return result;

                result.EligibleProductAmount = productSubtotal;

                var marketingDiscounts = await CalculateMarketingCampaignDiscountsAsync(
                    websiteId,
                    input,
                    productSubtotal,
                    now
                );

                var selectedDiscounts = SelectBestDiscountCombination(marketingDiscounts, productSubtotal);
                if (selectedDiscounts.Any())
                {
                    selectedDiscounts = OrderDiscounts(selectedDiscounts).ToList();
                    var remainingAmount = productSubtotal;
                    foreach (var candidate in selectedDiscounts)
                    {
                        var discount = candidate.Discount;
                        discount.BaseAmount = remainingAmount;

                        if (discount.RuleType == MarketingRuleTypeEnum.PercentDiscount)
                        {
                            discount.DiscountAmount = Math.Floor(
                                remainingAmount * (100 - candidate.DiscountPercent) / 100
                            );

                            if (candidate.MaxDiscountAmount > 0)
                                discount.DiscountAmount = Math.Min(discount.DiscountAmount, candidate.MaxDiscountAmount);
                        }

                        discount.DiscountAmount = Math.Max(0, Math.Min(discount.DiscountAmount, remainingAmount));
                        remainingAmount -= discount.DiscountAmount;
                        discount.DisplayText = discount.AppliedTimes > 1
                            ? $"行銷優惠：{discount.Name}，本次折抵 {discount.DiscountAmount:#,##0} 元，套用 {discount.AppliedTimes} 次"
                            : $"行銷優惠：{discount.Name}，本次折抵 {discount.DiscountAmount:#,##0} 元";
                    }

                    result.AppliedDiscounts = selectedDiscounts
                        .Select(x => x.Discount)
                        .Where(x => x.DiscountAmount > 0)
                        .ToList();
                    result.TotalDiscountAmount = result.AppliedDiscounts.Sum(x => x.DiscountAmount);
                    result.Memo = string.Join("；", result.AppliedDiscounts.Select(x => x.DisplayText));
                }

                result.TotalDiscountAmount = Math.Max(0, result.TotalDiscountAmount);
                result.TotalDiscountAmount = Math.Min(result.TotalDiscountAmount, productSubtotal);

                return result;
            }
            catch (Exception ex) when (IsMissingMarketingCampaignsTable(ex))
            {
                return new CheckoutDiscountResultDto();
            }
        }

        private static List<DiscountCandidate> SelectBestDiscountCombination(
            IReadOnlyList<DiscountCandidate> candidates,
            decimal productSubtotal)
        {
            if (!candidates.Any())
                return new List<DiscountCandidate>();

            var combinations = new List<List<DiscountCandidate>>();
            var stackable = candidates.Where(x => x.CanStack).ToList();
            if (stackable.Any())
                combinations.Add(stackable);

            combinations.AddRange(candidates
                .Where(x => !x.CanStack)
                .Select(x => new List<DiscountCandidate> { x }));

            return combinations
                .OrderByDescending(x => CalculateCombinationDiscount(x, productSubtotal))
                .ThenBy(x => x.Min(y => y.Priority))
                .ThenBy(x => x.Min(y => y.Sequence))
                .First();
        }

        private static IOrderedEnumerable<DiscountCandidate> OrderDiscounts(
            IEnumerable<DiscountCandidate> discounts)
        {
            return discounts
                .OrderBy(x => x.MinAmount)
                .ThenBy(x => x.Priority)
                .ThenBy(x => x.Sequence);
        }

        private static decimal CalculateCombinationDiscount(
            IEnumerable<DiscountCandidate> discounts,
            decimal productSubtotal)
        {
            var remainingAmount = productSubtotal;

            foreach (var candidate in OrderDiscounts(discounts))
            {
                var amount = candidate.Discount.DiscountAmount;

                if (candidate.Discount.RuleType == MarketingRuleTypeEnum.PercentDiscount)
                {
                    amount = Math.Floor(remainingAmount * (100 - candidate.DiscountPercent) / 100);

                    if (candidate.MaxDiscountAmount > 0)
                        amount = Math.Min(amount, candidate.MaxDiscountAmount);
                }

                amount = Math.Max(0, Math.Min(amount, remainingAmount));
                remainingAmount -= amount;
            }

            return productSubtotal - remainingAmount;
        }

        private async Task<List<DiscountCandidate>> CalculateMarketingCampaignDiscountsAsync(
            long websiteId,
            CheckoutDiscountInputDto input,
            decimal productSubtotal,
            DateTime now)
        {
            var output = new List<DiscountCandidate>();
            var sequence = 0;

            var campaigns = await db.MarketingCampaigns
                .AsNoTracking()
                .Include(x => x.Rules)
                    .ThenInclude(x => x.Condition)
                .Include(x => x.Rules)
                    .ThenInclude(x => x.Reward)
                .Where(x => !x.IsDeleted)
                .Where(x => x.FK_WebsiteId == websiteId)
                .Where(x => x.CampaignType == MarketingCampaignTypeEnum.滿額優惠)
                .Where(x => x.Status == MarketingDisplayStatusEnum.活動中)
                .Where(x => x.StartTime <= now)
                .Where(x => x.NeverEnd || (x.EndTime.HasValue && x.EndTime.Value >= now))
                .OrderBy(x => x.Priority)
                .ThenByDescending(x => x.CreationTime)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            foreach (var campaign in campaigns)
            {
                var rules = campaign.Rules
                    .Where(r => !r.IsDeleted)
                    .Where(r => r.Enabled)
                    .Where(r => r.ScopeType == MarketingScopeTypeEnum.AllOrder)
                    .Where(r => r.Condition != null && !r.Condition.IsDeleted)
                    .Where(r => r.Condition.ConditionType == MarketingConditionTypeEnum.OrderAmount)
                    .Where(r => r.Reward != null && !r.Reward.IsDeleted)
                    .Where(r => r.Reward.DeliveryType == MarketingRewardDeliveryTypeEnum.ApplyImmediately)
                    .Where(r =>
                        r.RuleType == MarketingRuleTypeEnum.AmountDiscount ||
                        r.RuleType == MarketingRuleTypeEnum.PercentDiscount)
                    .OrderBy(r => r.SortOrder)
                    .ToList();

                foreach (var rule in rules)
                {
                    var discount = CalculateOrderAmountRule(campaign, rule, productSubtotal);

                    if (discount != null && discount.DiscountAmount > 0)
                    {
                        output.Add(new DiscountCandidate
                        {
                            Discount = discount,
                            CanStack = campaign.CanStack,
                            Priority = campaign.Priority,
                            Sequence = sequence++,
                            MinAmount = rule.Condition?.MinAmount ?? 0,
                            DiscountPercent = rule.Reward?.DiscountPercent ?? 0,
                            MaxDiscountAmount = rule.Reward?.MaxDiscountAmount ?? 0
                        });
                    }
                }
            }

            return output;
        }

        private static bool IsMissingMarketingCampaignsTable(Exception ex)
        {
            return ex.Message.Contains("Invalid object name 'MarketingCampaigns'", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("無效的物件名稱 'MarketingCampaigns'", StringComparison.OrdinalIgnoreCase);
        }

        private static CheckoutDiscountAppliedDto? CalculateOrderAmountRule(
            Core.Models.MarketingCampaign campaign,
            Core.Models.MarketingRule rule,
            decimal productSubtotal)
        {
            var minAmount = rule.Condition?.MinAmount ?? 0;

            if (minAmount <= 0 || productSubtotal < minAmount)
                return null;

            var appliedTimes =
                rule.RuleType == MarketingRuleTypeEnum.AmountDiscount && campaign.Repeatable
                    ? (int)Math.Floor(productSubtotal / minAmount)
                    : 1;

            appliedTimes = Math.Max(1, appliedTimes);

            decimal discount = 0;

            switch (rule.RuleType)
            {
                case MarketingRuleTypeEnum.AmountDiscount:
                    var discountAmount = rule.Reward?.DiscountAmount ?? 0;

                    discount = campaign.Repeatable
                        ? discountAmount * appliedTimes
                        : discountAmount;
                    break;

                case MarketingRuleTypeEnum.PercentDiscount:
                    var percent = rule.Reward?.DiscountPercent ?? 0;

                    // 後台若填 90，代表 9 折，所以折扣金額是 10%
                    discount = Math.Floor(productSubtotal * (100 - percent) / 100);

                    if (rule.Reward?.MaxDiscountAmount != null && rule.Reward.MaxDiscountAmount.Value > 0)
                        discount = Math.Min(discount, rule.Reward.MaxDiscountAmount.Value);

                    break;
            }

            discount = Math.Max(0, discount);
            discount = Math.Min(discount, productSubtotal);

            if (discount <= 0)
                return null;

            var displayText = $"行銷優惠：{campaign.Name}，本次折抵 {discount:#,##0} 元";

            return new CheckoutDiscountAppliedDto
            {
                SourceType = CheckoutDiscountSourceTypeEnum.MarketingCampaign,
                CampaignId = campaign.Id,
                RuleId = rule.Id,
                Name = campaign.Name,
                CampaignType = campaign.CampaignType,
                RuleType = rule.RuleType,
                BaseAmount = productSubtotal,
                ThresholdAmount = minAmount,
                DiscountAmount = discount,
                AppliedTimes = appliedTimes,
                DisplayText = displayText
            };
        }
    }
}

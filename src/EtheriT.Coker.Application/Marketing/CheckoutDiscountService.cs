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
            var websiteId = await loginUserData.GetCommonWebsiteId();

            var now = DateTime.Now;

            var result = new CheckoutDiscountResultDto();

            if (input == null || input.Items == null || !input.Items.Any())
                return result;

            var productSubtotal = input.Items.Sum(x => x.Subtotal);

            if (productSubtotal <= 0)
                return result;

            var marketingDiscounts = await CalculateMarketingCampaignDiscountsAsync(
                websiteId,
                input,
                productSubtotal,
                now
            );

            foreach (var discount in marketingDiscounts)
            {
                result.AppliedDiscounts.Add(discount);
            }

            // 第一階段先只支援不疊加：取第一筆
            // 未來 CanStack 開始支援時，這裡改成套用策略。
            if (result.AppliedDiscounts.Any())
            {
                var selected = result.AppliedDiscounts.First();

                result.AppliedDiscounts = new List<CheckoutDiscountAppliedDto> { selected };
                result.TotalDiscountAmount = selected.DiscountAmount;
                result.Memo = selected.DisplayText;
            }

            result.TotalDiscountAmount = Math.Max(0, result.TotalDiscountAmount);
            result.TotalDiscountAmount = Math.Min(result.TotalDiscountAmount, productSubtotal);

            return result;
        }

        private async Task<List<CheckoutDiscountAppliedDto>> CalculateMarketingCampaignDiscountsAsync(
            long websiteId,
            CheckoutDiscountInputDto input,
            decimal productSubtotal,
            DateTime now)
        {
            var output = new List<CheckoutDiscountAppliedDto>();

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
                        output.Add(discount);
                }
            }

            return output;
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
                DiscountAmount = discount,
                AppliedTimes = appliedTimes,
                DisplayText = displayText
            };
        }
    }
}
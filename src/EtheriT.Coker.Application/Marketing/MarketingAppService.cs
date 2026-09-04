using AutoMapper;
using DevExpress.CodeParser;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;
using EtheriT.Coker.Application.Shared.Dto.Marketing;
using EtheriT.Coker.Application.Shared.Marketing;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EtheriT.Coker.Application.Marketing
{
    public class MarketingAppService : IMarketingAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly IFileUploadAppService fileUploadAppService;

        public MarketingAppService(
            CokerDbContext context,
            LoginUserData loginUserData,
            IMapper mapper,
            IFileUploadAppService fileUploadAppService)
        {
            db = context;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.fileUploadAppService = fileUploadAppService;
        }

        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            var websiteId = await loginUserData.GetWebsiteId();
            var now = DateTime.Now;

            var query = db.MarketingCampaigns
                .Where(x => x.FK_WebsiteId == websiteId)
                .OrderByDescending(x => x.LastModificationTime ?? x.CreationTime)
                .ThenByDescending(x => x.CreationTime)
                .ThenByDescending(x => x.Id)
                .Select(x => new MarketingCampaignListDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    CampaignType = x.CampaignType,

                    Status =
                        x.Status == MarketingDisplayStatusEnum.草稿
                            ? MarketingDisplayStatusEnum.草稿
                            : x.Status == MarketingDisplayStatusEnum.已關閉
                                ? MarketingDisplayStatusEnum.已關閉
                                : x.Status == MarketingDisplayStatusEnum.活動中 && now < x.StartTime
                                    ? MarketingDisplayStatusEnum.未開始
                                    : x.Status == MarketingDisplayStatusEnum.活動中 && !x.NeverEnd && x.EndTime.HasValue && now > x.EndTime.Value
                                        ? MarketingDisplayStatusEnum.已結束
                                        : x.Status,

                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    NeverEnd = x.NeverEnd,
                    Priority = x.Priority,
                    CanStack = x.CanStack,
                    Repeatable = x.Repeatable,

                    RuleType = x.Rules
                        .Where(r => r.Enabled)
                        .OrderBy(r => r.SortOrder)
                        .Select(r => r.RuleType)
                        .FirstOrDefault(),

                    ConditionType = x.Rules
                        .Where(r => r.Enabled)
                        .OrderBy(r => r.SortOrder)
                        .Select(r => r.Condition.ConditionType)
                        .FirstOrDefault(),

                    MinAmount = x.Rules
                        .Where(r => r.Enabled)
                        .OrderBy(r => r.SortOrder)
                        .Select(r => r.Condition.MinAmount)
                        .FirstOrDefault(),

                    MinQuantity = x.Rules
                        .Where(r => r.Enabled)
                        .OrderBy(r => r.SortOrder)
                        .Select(r => r.Condition.MinQuantity)
                        .FirstOrDefault(),

                    DiscountAmount = x.Rules
                        .Where(r => r.Enabled)
                        .OrderBy(r => r.SortOrder)
                        .Select(r => r.Reward.DiscountAmount)
                        .FirstOrDefault(),

                    DiscountPercent = x.Rules
                        .Where(r => r.Enabled)
                        .OrderBy(r => r.SortOrder)
                        .Select(r => r.Reward.DiscountPercent)
                        .FirstOrDefault(),

                    ScopeItemCount = x.Rules
                        .Where(r => r.Enabled)
                        .SelectMany(r => r.ScopeItems)
                        .Count(),

                    RewardItemCount = x.Rules
                        .Where(r => r.Enabled && r.Reward != null)
                        .SelectMany(r => r.Reward.Items)
                        .Count(i => i.Enabled),

                    MinOfferPrice = x.Rules
                        .Where(r => r.Enabled && r.Reward != null)
                        .SelectMany(r => r.Reward.Items)
                        .Where(i => i.Enabled)
                        .Select(i => (decimal?)i.OfferPrice)
                        .Min(),

                    MaxOfferPrice = x.Rules
                        .Where(r => r.Enabled && r.Reward != null)
                        .SelectMany(r => r.Reward.Items)
                        .Where(i => i.Enabled)
                        .Select(i => (decimal?)i.OfferPrice)
                        .Max()
                });

            var output = await DataSourceLoader.LoadAsync(query, loadOptions);

            return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }

        public async Task<ResponseMessageDto> GetOne(long id)
        {
            var websiteId = await loginUserData.GetWebsiteId();

            var campaign = await db.MarketingCampaigns
                .Include(x => x.Rules)
                    .ThenInclude(x => x.Condition)
                .Include(x => x.Rules)
                    .ThenInclude(x => x.Reward)
                        .ThenInclude(x => x.Items)
                            .ThenInclude(x => x.ProdStock)
                                .ThenInclude(x => x.Prod)
                .Include(x => x.Rules)
                    .ThenInclude(x => x.ScopeItems)
                .FirstOrDefaultAsync(x => x.Id == id && x.FK_WebsiteId == websiteId);

            if (campaign == null)
            {
                return new ResponseMessageDto
                {
                    Success = false,
                    Message = "找不到行銷活動資料。"
                };
            }

            var rule = campaign.Rules
                .OrderBy(x => x.SortOrder)
                .FirstOrDefault();

            var dto = mapper.Map<MarketingCampaignEditDto>(campaign);

            dto.RuleType = rule?.RuleType ?? MarketingRuleTypeEnum.AmountDiscount;
            dto.CanStack = rule?.RuleType == MarketingRuleTypeEnum.AddOnPurchase || campaign.CanStack;
            dto.ConditionType = rule?.Condition?.ConditionType ?? MarketingConditionTypeEnum.OrderAmount;
            dto.ScopeType = rule?.ScopeType ?? MarketingScopeTypeEnum.AllOrder;
            dto.MinAmount = rule?.Condition?.MinAmount;
            dto.MinQuantity = rule?.Condition?.MinQuantity;
            if (!dto.MinQuantity.HasValue &&
                rule?.Condition?.ConditionType == MarketingConditionTypeEnum.BuySpecificProduct)
            {
                dto.MinQuantity = rule.ScopeItems
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.Id)
                    .Select(x => (int?)x.RequiredQuantityPerQualification)
                    .FirstOrDefault() ?? 1;
            }
            dto.DiscountAmount = rule?.Reward?.DiscountAmount;
            dto.DiscountPercent = rule?.Reward?.DiscountPercent;
            dto.MaxDiscountAmount = rule?.Reward?.MaxDiscountAmount;
            dto.SelectionQuantityPerQualification = rule?.Reward?.SelectionQuantityPerQualification ?? 1;
            dto.MaxSelectionQuantityPerOrder = rule?.Reward?.MaxSelectionQuantityPerOrder;

            if (rule != null)
            {
                var productIds = rule.ScopeItems
                    .Where(x => x.TargetType == MarketingScopeTargetTypeEnum.Product)
                    .Select(x => x.TargetId)
                    .Distinct()
                    .ToList();
                var rewardProductIds = rule.Reward?.Items
                    .Where(x => x.Enabled)
                    .Select(x => x.ProdStock.FK_Pid)
                    .Distinct()
                    .ToList() ?? new List<long>();
                var imageMap = await fileUploadAppService.GetMinImageMapAsync(
                    productIds.Concat(rewardProductIds).Distinct().ToList());

                var productInfos = await db.Prods
                    .AsNoTracking()
                    .Where(x => productIds.Contains(x.Id) && x.FK_WebsiteId == websiteId)
                    .Select(x => new
                    {
                        x.Id,
                        x.Title,
                        x.Status,
                        x.Visible,
                        x.RemovedFromShelves,
                        x.NoStockManagement,
                        StockQuantity = x.Prod_Stocks
                            .Where(s => !s.IsDeleted)
                            .Sum(s => (int?)(s.Stock ?? 0)) ?? 0,
                        AlertQuantity = x.Prod_Stocks
                            .Where(s => !s.IsDeleted)
                            .Sum(s => (int?)(s.Alert_Qty ?? 0)) ?? 0
                    })
                    .ToDictionaryAsync(x => x.Id);

                dto.ScopeItems = rule.ScopeItems
                    .OrderBy(x => x.Id)
                    .Select(x =>
                    {
                        productInfos.TryGetValue(x.TargetId, out var product);
                        imageMap.TryGetValue(x.TargetId, out var imageUrl);
                        return new MarketingScopeItemEditDto
                        {
                            Id = x.Id,
                            TargetType = x.TargetType,
                            TargetId = x.TargetId,
                            TargetName = product?.Title ?? $"商品 #{x.TargetId}",
                            RequiredQuantityPerQualification = x.RequiredQuantityPerQualification,
                            ProductStatus = product == null ? 0 : (int)product.Status,
                            ProductStatusName = product?.Status.ToString() ?? "商品不存在",
                            Visible = product?.Visible ?? false,
                            Available = product != null && !product.RemovedFromShelves,
                            NoStockManagement = product?.NoStockManagement ?? false,
                            StockQuantity = product?.NoStockManagement == true ? null : product?.StockQuantity,
                            AlertQuantity = product?.NoStockManagement == true ? null : product?.AlertQuantity,
                            ImageUrl = imageUrl ?? "/images/noImg.jpg"
                        };
                    })
                    .ToList();

                var rewardStockIds = rule.Reward?.Items
                    .Where(x => x.Enabled)
                    .Select(x => x.FK_ProdStockId)
                    .Distinct()
                    .ToList() ?? new List<long>();
                var rewardSpecIds = rule.Reward?.Items
                    .Where(x => x.Enabled)
                    .SelectMany(x => new[] { x.ProdStock.FK_S1id, x.ProdStock.FK_S2id })
                    .Where(x => x.HasValue && x.Value > 0)
                    .Select(x => x!.Value)
                    .Distinct()
                    .ToList() ?? new List<long>();
                var rewardSpecNames = rewardSpecIds.Any()
                    ? await db.Prod_Specs.AsNoTracking()
                        .Where(x => rewardSpecIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id, x => x.Title)
                    : new Dictionary<long, string>();
                var rewardCashPrices = await db.Prod_Prices
                    .AsNoTracking()
                    .Where(x => rewardStockIds.Contains(x.FK_PSId) && !x.IsDeleted &&
                                (x.Bonus ?? 0) == 0 && x.Price.HasValue)
                    .Select(x => new { StockId = x.FK_PSId, RoleId = x.FK_RId, Price = x.Price!.Value })
                    .ToListAsync();
                var originalPriceMap = rewardCashPrices
                    .GroupBy(x => x.StockId)
                    .ToDictionary(
                        x => x.Key,
                        x => x.OrderBy(y => y.RoleId is 0 or 1 ? 0 : 1)
                              .ThenBy(y => y.RoleId)
                              .Select(y => y.Price)
                              .First());

                dto.RewardItems = rule.Reward?.Items
                    .Where(x => x.Enabled)
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.Id)
                    .Select(x =>
                    {
                        var originalPrice = originalPriceMap.TryGetValue(x.FK_ProdStockId, out var price)
                            ? price
                            : x.ProdStock.Price;
                        imageMap.TryGetValue(x.ProdStock.FK_Pid, out var imageUrl);
                        return new MarketingRewardItemEditDto
                        {
                            Id = x.Id,
                            ProductId = x.ProdStock.FK_Pid,
                            ProductStockId = x.FK_ProdStockId,
                            ProductName = x.ProdStock.Prod?.Title ?? $"商品 #{x.ProdStock.FK_Pid}",
                            StockName = BuildStockName(x.ProdStock, rewardSpecNames),
                            Sku = x.ProdStock.SubItemNo ?? string.Empty,
                            OriginalPrice = originalPrice,
                            ProductStatus = x.ProdStock.Prod == null ? 0 : (int)x.ProdStock.Prod.Status,
                            ProductStatusName = x.ProdStock.Prod?.Status.ToString() ?? "商品不存在",
                            Visible = x.ProdStock.Prod?.Visible ?? false,
                            Available = x.ProdStock.Prod != null && !x.ProdStock.Prod.RemovedFromShelves,
                            NoStockManagement = x.ProdStock.Prod?.NoStockManagement ?? false,
                            StockQuantity = x.ProdStock.Prod?.NoStockManagement == true ? null : x.ProdStock.Stock,
                            AlertQuantity = x.ProdStock.Prod?.NoStockManagement == true ? null : x.ProdStock.Alert_Qty,
                            ImageUrl = imageUrl ?? "/images/noImg.jpg",
                            OfferPrice = x.OfferPrice,
                            MaxQuantityPerOrder = x.MaxQuantityPerOrder,
                            Enabled = x.Enabled,
                            SortOrder = x.SortOrder
                        };
                    })
                    .ToList() ?? new List<MarketingRewardItemEditDto>();
            }

            return new ResponseMessageDto
            {
                Success = true,
                Object = dto
            };
        }

        public async Task<ResponseMessageDto> AddUp(MarketingCampaignEditDto input)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var validateMessage = ValidateInput(input);
                if (!string.IsNullOrWhiteSpace(validateMessage))
                {
                    throw new Exception(validateMessage);
                }

                var websiteId = await loginUserData.GetWebsiteId();

                var isOrderDiscount = input.RuleType == MarketingRuleTypeEnum.AmountDiscount ||
                                      input.RuleType == MarketingRuleTypeEnum.PercentDiscount;
                if (isOrderDiscount &&
                    input.Status != MarketingDisplayStatusEnum.已結束 &&
                    input.Status != MarketingDisplayStatusEnum.已關閉 &&
                    input.CanStack &&
                    !input.StackingConflictConfirmed)
                {
                    var inputEndTime = input.NeverEnd || !input.EndTime.HasValue
                        ? DateTime.MaxValue
                        : input.EndTime.Value;
                    var stackingConflicts = await db.MarketingCampaigns
                        .AsNoTracking()
                        .Where(x => !x.IsDeleted && x.Id != input.Id && x.FK_WebsiteId == websiteId)
                        .Where(x => x.CampaignType == MarketingCampaignTypeEnum.滿額優惠 &&
                                    x.Status != MarketingDisplayStatusEnum.已結束 &&
                                    x.Status != MarketingDisplayStatusEnum.已關閉 &&
                                    x.CanStack)
                        .Where(x => x.StartTime <= inputEndTime &&
                                    (x.NeverEnd || !x.EndTime.HasValue || x.EndTime.Value >= input.StartTime))
                        .OrderBy(x => x.Priority)
                        .ThenBy(x => x.StartTime)
                        .Select(x => new
                        {
                            x.Id,
                            x.Name,
                            x.StartTime,
                            x.EndTime,
                            x.NeverEnd
                        })
                        .ToListAsync();

                    if (stackingConflicts.Any())
                    {
                        response.Success = false;
                        response.Error = "MarketingStackingConfirmationRequired";
                        response.Message = "相同期間已有可併用的滿額優惠，啟用後可能重複抵扣，確認後才會儲存。";
                        response.Object = stackingConflicts;
                        return response;
                    }
                }

                MarketingCampaign? campaign;

                if (input.Id > 0)
                {
                    campaign = await db.MarketingCampaigns
                        .Include(x => x.Rules)
                            .ThenInclude(x => x.Condition)
                        .Include(x => x.Rules)
                            .ThenInclude(x => x.Reward)
                                .ThenInclude(x => x.Items)
                        .Include(x => x.Rules)
                            .ThenInclude(x => x.ScopeItems)
                        .FirstOrDefaultAsync(x => x.Id == input.Id && x.FK_WebsiteId == websiteId);

                    if (campaign == null)
                    {
                        throw new Exception("找不到行銷活動資料，或您沒有權限編輯此活動。");
                    }

                    var existingRule = campaign.Rules
                        .Where(x => !x.IsDeleted && x.Enabled)
                        .OrderBy(x => x.SortOrder)
                        .FirstOrDefault();
                    if (campaign.Status != MarketingDisplayStatusEnum.草稿 &&
                        existingRule != null &&
                        !IsSameRuleMode(existingRule, input))
                    {
                        throw new Exception("非草稿活動不可變更優惠類型；如需其他類型，請新增活動。");
                    }

                    mapper.Map(input, campaign);

                    campaign.EndTime = input.NeverEnd ? null : input.EndTime;
                    campaign.LastModificationTime = DateTime.Now;
                }
                else
                {
                    campaign = mapper.Map<MarketingCampaign>(input);

                    campaign.FK_WebsiteId = websiteId;
                    campaign.EndTime = input.NeverEnd ? null : input.EndTime;
                    campaign.CreationTime = DateTime.Now;
                    campaign.Rules = new List<MarketingRule>();

                    db.MarketingCampaigns.Add(campaign);
                }

                var rule = campaign.Rules
                    .OrderBy(x => x.SortOrder)
                    .FirstOrDefault();

                if (rule == null)
                {
                    rule = new MarketingRule
                    {
                        MarketingCampaign = campaign,
                        ScopeType = MarketingScopeTypeEnum.AllOrder,
                        Enabled = true,
                        SortOrder = 0,
                        Condition = new MarketingCondition(),
                        Reward = new MarketingReward()
                    };

                    campaign.Rules.Add(rule);
                }

                rule.RuleType = input.RuleType;
                rule.Enabled = true;
                campaign.CanStack = input.RuleType == MarketingRuleTypeEnum.AddOnPurchase || input.CanStack;

                if (rule.Condition == null)
                {
                    rule.Condition = new MarketingCondition
                    {
                        MarketingRule = rule
                    };
                }
                if (rule.Reward == null)
                {
                    rule.Reward = new MarketingReward
                    {
                        MarketingRule = rule
                    };
                }

                rule.Reward.DeliveryType = MarketingRewardDeliveryTypeEnum.ApplyImmediately;

                if (input.RuleType == MarketingRuleTypeEnum.AddOnPurchase)
                {
                    campaign.CampaignType = MarketingCampaignTypeEnum.加價購;
                    ApplyAddOnRule(input, rule);
                    await SyncScopeItemsAsync(input, rule, websiteId);
                    await SyncRewardItemsAsync(input, rule, websiteId);
                }
                else
                {
                    campaign.CampaignType = MarketingCampaignTypeEnum.滿額優惠;
                    ApplyOrderDiscountRule(input, rule);
                    SoftDeleteScopeItems(rule.ScopeItems);
                    SoftDeleteRewardItems(rule.Reward.Items);
                }

                await db.SaveChangesAsync();

                response.Success = true;
                response.Object = campaign.Id;
                response.Message = "儲存成功。";
            }
            catch (Exception ex)
            {
                response.Message = "儲存失敗。";
                response.Error = ex.Message;
            }
            finally
            {
                await loginUserData.SetLogs(
                    JsonConvert.SerializeObject(input),
                    JsonConvert.SerializeObject(response)
                );
            }
            return response;
        }

        public async Task<ResponseMessageDto> Delete(long id)
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                var websiteId = await loginUserData.GetWebsiteId();

                var campaign = await db.MarketingCampaigns
                    .Include(x => x.Rules)
                        .ThenInclude(x => x.Condition)
                    .Include(x => x.Rules)
                        .ThenInclude(x => x.ScopeItems)
                    .Include(x => x.Rules)
                        .ThenInclude(x => x.Reward)
                            .ThenInclude(x => x.Items)
                                .ThenInclude(x => x.ProdStock)
                                    .ThenInclude(x => x.Prod)
                    .FirstOrDefaultAsync(x => x.Id == id && x.FK_WebsiteId == websiteId);

                if (campaign == null)
                {
                    throw new Exception("找不到行銷活動資料，或您沒有權限刪除此活動。");
                }

                campaign.IsDeleted = true;
                campaign.DeletionTime = DateTime.Now;

                foreach (var rule in campaign.Rules)
                {
                    rule.IsDeleted = true;
                    rule.DeletionTime = DateTime.Now;

                    if (rule.Condition != null)
                    {
                        rule.Condition.IsDeleted = true;
                        rule.Condition.DeletionTime = DateTime.Now;
                    }

                    if (rule.Reward != null)
                    {
                        rule.Reward.IsDeleted = true;
                        rule.Reward.DeletionTime = DateTime.Now;

                        foreach (var rewardItem in rule.Reward.Items)
                        {
                            rewardItem.IsDeleted = true;
                            rewardItem.DeletionTime = DateTime.Now;
                        }
                    }

                    foreach (var scopeItem in rule.ScopeItems)
                    {
                        scopeItem.IsDeleted = true;
                        scopeItem.DeletionTime = DateTime.Now;
                    }
                }

                await db.SaveChangesAsync();
                response.Success = true;
                response.Message = "刪除成功。";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "刪除失敗。";
                response.Error = ex.Message;
            }
            finally
            {
                await loginUserData.SetLogs(
                    JsonConvert.SerializeObject(new {id}),
                    JsonConvert.SerializeObject(response)
                );
            }
            return response;
        }

        public Task<ResponseMessageDto> GetOptions()
        {
            var options = new MarketingOptionsDto
            {
                CampaignTypes = new List<LookUpItemDto>
                {
                    new() { Value = (int)MarketingCampaignTypeEnum.滿額優惠, Text = "滿額優惠" },
                    new() { Value = (int)MarketingCampaignTypeEnum.滿件優惠, Text = "滿件優惠" },
                    new() { Value = (int)MarketingCampaignTypeEnum.指定商品優惠, Text = "指定商品優惠" },
                    new() { Value = (int)MarketingCampaignTypeEnum.加價購, Text = "加價購" },
                    new() { Value = (int)MarketingCampaignTypeEnum.贈品活動, Text = "贈品活動" },
                    new() { Value = (int)MarketingCampaignTypeEnum.免運活動, Text = "免運活動" },
                    new() { Value = (int)MarketingCampaignTypeEnum.推薦商品, Text = "推薦商品" }
                },

                RuleTypes = new List<LookUpItemDto>
                {
                    new() { Value = (int)MarketingRuleTypeEnum.AmountDiscount, Text = "滿額折固定金額" },
                    new() { Value = (int)MarketingRuleTypeEnum.PercentDiscount, Text = "滿額打折" },
                    new() { Value = (int)MarketingRuleTypeEnum.AddOnPurchase, Text = "加價購／贈品" }
                },

                DisplayStatuses = new List<LookUpItemDto>
                {
                    new() { Value = (int)MarketingDisplayStatusEnum.草稿, Text = "草稿" },
                    new() { Value = (int)MarketingDisplayStatusEnum.未開始, Text = "未開始" },
                    new() { Value = (int)MarketingDisplayStatusEnum.活動中, Text = "活動中" },
                    new() { Value = (int)MarketingDisplayStatusEnum.已結束, Text = "已結束" },
                    new() { Value = (int)MarketingDisplayStatusEnum.已關閉, Text = "已關閉" }
                },

                EditableStatuses = new List<LookUpItemDto>
                {
                    new() { Value = (int)MarketingDisplayStatusEnum.草稿, Text = "草稿" },
                    new() { Value = (int)MarketingDisplayStatusEnum.活動中, Text = "活動中" },
                    new() { Value = (int)MarketingDisplayStatusEnum.已關閉, Text = "已關閉" }
                }
            };

            return Task.FromResult(new ResponseMessageDto
            {
                Success = true,
                Object = options
            });
        }

        public async Task<ResponseMessageDto> GetCartMarketingCampaigns()
        {
            try
            {
                var websiteId = await loginUserData.GetCommonWebsiteId();
                var now = DateTime.Now;

                var campaigns = await db.MarketingCampaigns
                    .AsNoTracking()
                    .Include(x => x.Rules)
                        .ThenInclude(x => x.Condition)
                    .Include(x => x.Rules)
                        .ThenInclude(x => x.ScopeItems)
                    .Include(x => x.Rules)
                        .ThenInclude(x => x.Reward)
                            .ThenInclude(x => x.Items)
                                .ThenInclude(x => x.ProdStock)
                                    .ThenInclude(x => x.Prod)
                    .Where(x => !x.IsDeleted)
                    .Where(x => x.FK_WebsiteId == websiteId)
                    .Where(x => x.Status == MarketingDisplayStatusEnum.活動中)
                    .Where(x => x.StartTime <= now)
                    .Where(x => x.NeverEnd || (x.EndTime.HasValue && x.EndTime.Value >= now))
                    .OrderBy(x => x.Priority)
                    .ThenByDescending(x => x.CreationTime)
                    .ThenByDescending(x => x.Id)
                    .ToListAsync();

                var output = new CartMarketingCampaignsDto();

                var addOnRules = campaigns
                    .SelectMany(x => x.Rules)
                    .Where(x => !x.IsDeleted && x.Enabled &&
                                x.RuleType == MarketingRuleTypeEnum.AddOnPurchase &&
                                x.Condition != null && !x.Condition.IsDeleted &&
                                (x.Condition.ConditionType == MarketingConditionTypeEnum.OrderAmount ||
                                 x.Condition.ConditionType == MarketingConditionTypeEnum.ScopeAmount ||
                                 x.Condition.ConditionType == MarketingConditionTypeEnum.ScopeQuantity ||
                                 x.Condition.ConditionType == MarketingConditionTypeEnum.BuySpecificProduct) &&
                                x.Reward != null && !x.Reward.IsDeleted)
                    .ToList();
                var rewardStockIds = addOnRules
                    .SelectMany(x => x.Reward.Items)
                    .Where(x => !x.IsDeleted && x.Enabled)
                    .Select(x => x.FK_ProdStockId)
                    .Distinct()
                    .ToList();
                var rewardProductIds = addOnRules
                    .SelectMany(x => x.Reward.Items)
                    .Where(x => !x.IsDeleted && x.Enabled && x.ProdStock?.Prod != null)
                    .Select(x => x.ProdStock.FK_Pid)
                    .Distinct()
                    .ToList();
                var scopeProductIds = addOnRules
                    .SelectMany(x => x.ScopeItems)
                    .Where(x => !x.IsDeleted && x.TargetType == MarketingScopeTargetTypeEnum.Product)
                    .Select(x => x.TargetId)
                    .Distinct()
                    .ToList();
                var imageMap = await fileUploadAppService.GetMinImageMapAsync(
                    rewardProductIds.Concat(scopeProductIds).Distinct().ToList());
                var scopeProductMap = await db.Prods.AsNoTracking()
                    .Where(x => scopeProductIds.Contains(x.Id) && x.FK_WebsiteId == websiteId && !x.IsDeleted)
                    .Select(x => new
                    {
                        x.Id,
                        x.Title,
                        Available = x.Visible && !x.RemovedFromShelves
                    })
                    .ToDictionaryAsync(x => x.Id);
                var rewardPrices = await db.Prod_Prices.AsNoTracking()
                    .Where(x => rewardStockIds.Contains(x.FK_PSId) && !x.IsDeleted &&
                                (x.Bonus ?? 0) == 0 && x.Price.HasValue)
                    .Select(x => new { StockId = x.FK_PSId, RoleId = x.FK_RId, Price = x.Price!.Value })
                    .ToListAsync();
                var originalPriceMap = rewardPrices
                    .GroupBy(x => x.StockId)
                    .ToDictionary(x => x.Key, x => x.OrderBy(y => y.RoleId is 0 or 1 ? 0 : 1)
                        .ThenBy(y => y.RoleId).Select(y => y.Price).First());
                var specIds = addOnRules
                    .SelectMany(x => x.Reward.Items)
                    .Where(x => !x.IsDeleted && x.Enabled && x.ProdStock != null)
                    .SelectMany(x => new[] { x.ProdStock.FK_S1id, x.ProdStock.FK_S2id })
                    .Where(x => x.HasValue && x.Value > 0)
                    .Select(x => x!.Value)
                    .Distinct()
                    .ToList();
                var specNames = specIds.Any()
                    ? await db.Prod_Specs.AsNoTracking().Where(x => specIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id, x => x.Title)
                    : new Dictionary<long, string>();

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
                        .Select(r => new CartMarketingRuleDto
                        {
                            Id = r.Id,
                            RuleType = r.RuleType,
                            ScopeType = r.ScopeType,
                            MinAmount = r.Condition.MinAmount,
                            DiscountAmount = r.Reward.DiscountAmount,
                            DiscountPercent = r.Reward.DiscountPercent,
                            MaxDiscountAmount = r.Reward.MaxDiscountAmount
                        })
                        .ToList();

                    if (!rules.Any())
                        rules = new List<CartMarketingRuleDto>();

                    if (rules.Any()) output.OrderDiscounts.Add(new CartMarketingCampaignDto
                    {
                        Id = campaign.Id,
                        Name = campaign.Name,
                        CampaignType = campaign.CampaignType,
                        Priority = campaign.Priority,
                        CanStack = campaign.CanStack,
                        Repeatable = campaign.Repeatable,
                        Rules = rules
                    });

                    foreach (var rule in campaign.Rules.Where(x => addOnRules.Contains(x)).OrderBy(x => x.SortOrder))
                    {
                        var rewardItems = rule.Reward.Items
                            .Where(x => !x.IsDeleted && x.Enabled && x.ProdStock != null && !x.ProdStock.IsDeleted &&
                                        x.ProdStock.Prod != null && !x.ProdStock.Prod.IsDeleted &&
                                        x.ProdStock.Prod.Visible && !x.ProdStock.Prod.RemovedFromShelves &&
                                        x.ProdStock.Visible &&
                                        (x.ProdStock.Prod.NoStockManagement || (x.ProdStock.Stock ?? 0) > 0))
                            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
                            .Select(x =>
                            {
                                imageMap.TryGetValue(x.ProdStock.FK_Pid, out var imageUrl);
                                return new ProductAddOnRewardItemDto
                                {
                                    RewardItemId = x.Id,
                                    ProductId = x.ProdStock.FK_Pid,
                                    ProductStockId = x.FK_ProdStockId,
                                    ProductName = x.ProdStock.Prod.Title,
                                    StockName = BuildStockName(x.ProdStock, specNames),
                                    ImageUrl = imageUrl ?? "/images/noImg.jpg",
                                    OriginalPrice = originalPriceMap.TryGetValue(x.FK_ProdStockId, out var price) ? price : x.ProdStock.Price,
                                    OfferPrice = x.OfferPrice,
                                    MaxQuantityPerOrder = Math.Max(x.MaxQuantityPerOrder, 1),
                                    StockQuantity = x.ProdStock.Prod.NoStockManagement ? null : x.ProdStock.Stock,
                                    NoStockManagement = x.ProdStock.Prod.NoStockManagement
                                };
                            }).ToList();

                        var isQuantityCondition = rule.Condition.ConditionType == MarketingConditionTypeEnum.ScopeQuantity ||
                                                  rule.Condition.ConditionType == MarketingConditionTypeEnum.BuySpecificProduct;
                        if (!rewardItems.Any() ||
                            (isQuantityCondition && (!rule.Condition.MinQuantity.HasValue || rule.Condition.MinQuantity <= 0)) ||
                            (!isQuantityCondition && (!rule.Condition.MinAmount.HasValue || rule.Condition.MinAmount <= 0)))
                            continue;

                        output.AddOnCampaigns.Add(new CartAddOnCampaignDto
                        {
                            CampaignId = campaign.Id,
                            RuleId = rule.Id,
                            Name = campaign.Name,
                            Description = campaign.Description,
                            ConditionType = rule.Condition.ConditionType,
                            MinAmount = rule.Condition.MinAmount ?? 0,
                            RequiredQuantity = Math.Max(rule.Condition.MinQuantity ?? 1, 1),
                            Repeatable = campaign.Repeatable,
                            SelectionQuantityPerQualification = Math.Max(rule.Reward.SelectionQuantityPerQualification, 1),
                            ScopeProductIds = rule.ScopeItems.Where(x => !x.IsDeleted && x.TargetType == MarketingScopeTargetTypeEnum.Product)
                                .Select(x => x.TargetId).Distinct().ToList(),
                            ScopeProducts = rule.ScopeItems
                                .Where(x => !x.IsDeleted && x.TargetType == MarketingScopeTargetTypeEnum.Product)
                                .Select(x => x.TargetId)
                                .Distinct()
                                .Select(productId =>
                                {
                                    scopeProductMap.TryGetValue(productId, out var product);
                                    imageMap.TryGetValue(productId, out var imageUrl);
                                    return new CartAddOnScopeProductDto
                                    {
                                        ProductId = productId,
                                        ProductName = product?.Title ?? $"商品 #{productId}",
                                        ImageUrl = imageUrl ?? "/images/noImg.jpg",
                                        Available = product?.Available ?? false
                                    };
                                })
                                .ToList(),
                            RewardItems = rewardItems
                        });
                    }
                }

                return new ResponseMessageDto
                {
                    Success = true,
                    Object = output
                };
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Invalid object name 'MarketingCampaigns'", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("無效的物件名稱 'MarketingCampaigns'", StringComparison.OrdinalIgnoreCase))
                {
                    return new ResponseMessageDto
                    {
                        Success = true,
                        Object = new CartMarketingCampaignsDto()
                    };
                }

                return new ResponseMessageDto
                {
                    Success = false,
                    Message = "取得購物車行銷活動發生錯誤。",
                    Error = ex.Message
                };
            }
        }

        private static void ApplyOrderDiscountRule(MarketingCampaignEditDto input, MarketingRule rule)
        {
            rule.ScopeType = MarketingScopeTypeEnum.AllOrder;
            rule.Condition.ConditionType = MarketingConditionTypeEnum.OrderAmount;
            rule.Condition.MinAmount = input.MinAmount;
            rule.Condition.MinQuantity = null;
            rule.Condition.OnlyScopeItems = false;
            rule.Condition.ExcludeDiscountedItems = false;

            rule.Reward.DiscountAmount = input.RuleType == MarketingRuleTypeEnum.AmountDiscount
                ? input.DiscountAmount
                : null;
            rule.Reward.DiscountPercent = input.RuleType == MarketingRuleTypeEnum.PercentDiscount
                ? input.DiscountPercent
                : null;
            rule.Reward.MaxDiscountAmount = input.RuleType == MarketingRuleTypeEnum.PercentDiscount
                ? input.MaxDiscountAmount
                : null;
            rule.Reward.SelectionQuantityPerQualification = 1;
            rule.Reward.MaxSelectionQuantityPerOrder = null;
        }

        public async Task<ResponseMessageDto> GetProductAddOnCampaigns(long productId)
        {
            try
            {
                var websiteId = await loginUserData.GetCommonWebsiteId();
                var now = DateTime.Now;

                var campaigns = await db.MarketingCampaigns
                    .AsNoTracking()
                    .Include(x => x.Rules)
                        .ThenInclude(x => x.Condition)
                    .Include(x => x.Rules)
                        .ThenInclude(x => x.ScopeItems)
                    .Include(x => x.Rules)
                        .ThenInclude(x => x.Reward)
                            .ThenInclude(x => x.Items)
                                .ThenInclude(x => x.ProdStock)
                                    .ThenInclude(x => x.Prod)
                    .Where(x => !x.IsDeleted && x.FK_WebsiteId == websiteId)
                    .Where(x => x.Status == MarketingDisplayStatusEnum.活動中)
                    .Where(x => x.StartTime <= now)
                    .Where(x => x.NeverEnd || (x.EndTime.HasValue && x.EndTime.Value >= now))
                    .Where(x => x.Rules.Any(r =>
                        !r.IsDeleted && r.Enabled &&
                        r.RuleType == MarketingRuleTypeEnum.AddOnPurchase &&
                        r.Condition != null && !r.Condition.IsDeleted &&
                        (r.Condition.ConditionType == MarketingConditionTypeEnum.ScopeQuantity ||
                         r.Condition.ConditionType == MarketingConditionTypeEnum.BuySpecificProduct) &&
                        r.ScopeItems.Any(s => !s.IsDeleted &&
                            s.TargetType == MarketingScopeTargetTypeEnum.Product &&
                            s.TargetId == productId)))
                    .OrderBy(x => x.Priority)
                    .ThenBy(x => x.Id)
                    .ToListAsync();

                var rewardStockIds = campaigns
                    .SelectMany(x => x.Rules)
                    .Where(x => !x.IsDeleted && x.Enabled && x.Reward != null && !x.Reward.IsDeleted)
                    .SelectMany(x => x.Reward.Items)
                    .Where(x => !x.IsDeleted && x.Enabled)
                    .Select(x => x.FK_ProdStockId)
                    .Distinct()
                    .ToList();
                var rewardProductIds = campaigns
                    .SelectMany(x => x.Rules)
                    .Where(x => !x.IsDeleted && x.Enabled && x.Reward != null && !x.Reward.IsDeleted)
                    .SelectMany(x => x.Reward.Items)
                    .Where(x => !x.IsDeleted && x.Enabled && x.ProdStock?.Prod != null)
                    .Select(x => x.ProdStock.FK_Pid)
                    .Distinct()
                    .ToList();
                var imageMap = await fileUploadAppService.GetMinImageMapAsync(rewardProductIds);
                var cashPrices = await db.Prod_Prices
                    .AsNoTracking()
                    .Where(x => rewardStockIds.Contains(x.FK_PSId) && !x.IsDeleted &&
                                (x.Bonus ?? 0) == 0 && x.Price.HasValue)
                    .Select(x => new { StockId = x.FK_PSId, RoleId = x.FK_RId, Price = x.Price!.Value })
                    .ToListAsync();
                var originalPriceMap = cashPrices
                    .GroupBy(x => x.StockId)
                    .ToDictionary(
                        x => x.Key,
                        x => x.OrderBy(y => y.RoleId is 0 or 1 ? 0 : 1)
                              .ThenBy(y => y.RoleId)
                              .Select(y => y.Price)
                              .First());
                var rewardSpecIds = campaigns
                    .SelectMany(x => x.Rules)
                    .Where(x => !x.IsDeleted && x.Enabled && x.Reward != null && !x.Reward.IsDeleted)
                    .SelectMany(x => x.Reward.Items)
                    .Where(x => !x.IsDeleted && x.Enabled)
                    .SelectMany(x => new[] { x.ProdStock.FK_S1id, x.ProdStock.FK_S2id })
                    .Where(x => x.HasValue && x.Value > 0)
                    .Select(x => x!.Value)
                    .Distinct()
                    .ToList();
                var rewardSpecNames = rewardSpecIds.Any()
                    ? await db.Prod_Specs.AsNoTracking()
                        .Where(x => rewardSpecIds.Contains(x.Id))
                        .ToDictionaryAsync(x => x.Id, x => x.Title)
                    : new Dictionary<long, string>();

                var output = new List<ProductAddOnCampaignDto>();
                foreach (var campaign in campaigns)
                {
                    foreach (var rule in campaign.Rules
                                 .Where(x => !x.IsDeleted && x.Enabled &&
                                             x.RuleType == MarketingRuleTypeEnum.AddOnPurchase &&
                                             x.Condition != null && !x.Condition.IsDeleted &&
                                             (x.Condition.ConditionType == MarketingConditionTypeEnum.ScopeQuantity ||
                                              x.Condition.ConditionType == MarketingConditionTypeEnum.BuySpecificProduct) &&
                                             x.Reward != null && !x.Reward.IsDeleted)
                                 .OrderBy(x => x.SortOrder))
                    {
                        if (!rule.ScopeItems.Any(x => !x.IsDeleted &&
                                x.TargetType == MarketingScopeTargetTypeEnum.Product &&
                                x.TargetId == productId))
                        {
                            continue;
                        }

                        var items = rule.Reward.Items
                            .Where(x => !x.IsDeleted && x.Enabled &&
                                        x.ProdStock != null && !x.ProdStock.IsDeleted &&
                                        x.ProdStock.Prod != null && !x.ProdStock.Prod.IsDeleted &&
                                        x.ProdStock.Visible && x.ProdStock.Prod.Visible && !x.ProdStock.Prod.RemovedFromShelves &&
                                        (x.ProdStock.Prod.NoStockManagement || (x.ProdStock.Stock ?? 0) > 0))
                            .OrderBy(x => x.SortOrder)
                            .ThenBy(x => x.Id)
                            .Select(x =>
                            {
                                imageMap.TryGetValue(x.ProdStock.FK_Pid, out var imageUrl);
                                return new ProductAddOnRewardItemDto
                                {
                                    RewardItemId = x.Id,
                                    ProductId = x.ProdStock.FK_Pid,
                                    ProductStockId = x.FK_ProdStockId,
                                    ProductName = x.ProdStock.Prod!.Title,
                                    StockName = BuildStockName(x.ProdStock, rewardSpecNames),
                                    ImageUrl = imageUrl ?? "/images/noImg.jpg",
                                    OriginalPrice = originalPriceMap.TryGetValue(x.FK_ProdStockId, out var price)
                                        ? price
                                        : x.ProdStock.Price,
                                    OfferPrice = x.OfferPrice,
                                    MaxQuantityPerOrder = Math.Max(x.MaxQuantityPerOrder, 1),
                                    StockQuantity = x.ProdStock.Prod.NoStockManagement ? null : x.ProdStock.Stock,
                                    NoStockManagement = x.ProdStock.Prod.NoStockManagement
                                };
                            })
                            .ToList();

                        if (!items.Any())
                            continue;

                        output.Add(new ProductAddOnCampaignDto
                        {
                            CampaignId = campaign.Id,
                            RuleId = rule.Id,
                            Name = campaign.Name,
                            Description = campaign.Description,
                            RequiredQuantity = Math.Max(rule.Condition.MinQuantity ?? 1, 1),
                            SelectionQuantityPerQualification = Math.Max(rule.Reward.SelectionQuantityPerQualification, 1),
                            Repeatable = campaign.Repeatable,
                            ScopeProductIds = rule.ScopeItems
                                .Where(x => !x.IsDeleted && x.TargetType == MarketingScopeTargetTypeEnum.Product)
                                .Select(x => x.TargetId)
                                .Distinct()
                                .ToList(),
                            RewardItems = items
                        });
                    }
                }

                return new ResponseMessageDto { Success = true, Object = output };
            }
            catch (Exception ex)
            {
                return new ResponseMessageDto
                {
                    Success = false,
                    Message = "取得商品加價購活動發生錯誤。",
                    Error = ex.Message
                };
            }
        }

        private static bool IsSameRuleMode(MarketingRule existingRule, MarketingCampaignEditDto input)
        {
            if (existingRule.RuleType != input.RuleType)
                return false;

            if (existingRule.RuleType != MarketingRuleTypeEnum.AddOnPurchase)
                return true;

            var existingCondition = NormalizeAddOnCondition(existingRule.Condition?.ConditionType
                ?? MarketingConditionTypeEnum.OrderAmount);
            var inputCondition = NormalizeAddOnCondition(input.ConditionType);
            return existingCondition == inputCondition;
        }

        private static MarketingConditionTypeEnum NormalizeAddOnCondition(MarketingConditionTypeEnum conditionType)
        {
            return conditionType == MarketingConditionTypeEnum.BuySpecificProduct
                ? MarketingConditionTypeEnum.ScopeQuantity
                : conditionType;
        }

        private static void ApplyAddOnRule(MarketingCampaignEditDto input, MarketingRule rule)
        {
            rule.RuleType = MarketingRuleTypeEnum.AddOnPurchase;
            rule.ScopeType = input.ConditionType == MarketingConditionTypeEnum.OrderAmount
                ? MarketingScopeTypeEnum.AllOrder
                : MarketingScopeTypeEnum.SpecificProducts;

            rule.Condition.ConditionType = input.ConditionType;
            rule.Condition.MinAmount = input.ConditionType == MarketingConditionTypeEnum.OrderAmount ||
                                       input.ConditionType == MarketingConditionTypeEnum.ScopeAmount
                ? input.MinAmount
                : null;
            rule.Condition.MinQuantity = input.ConditionType == MarketingConditionTypeEnum.ScopeQuantity ||
                                         input.ConditionType == MarketingConditionTypeEnum.BuySpecificProduct
                ? input.MinQuantity
                : null;
            rule.Condition.OnlyScopeItems = input.ConditionType == MarketingConditionTypeEnum.ScopeAmount ||
                                            input.ConditionType == MarketingConditionTypeEnum.ScopeQuantity;
            rule.Condition.ExcludeDiscountedItems = false;

            rule.Reward.DiscountAmount = null;
            rule.Reward.DiscountPercent = null;
            rule.Reward.MaxDiscountAmount = null;
            rule.Reward.SelectionQuantityPerQualification = input.SelectionQuantityPerQualification;
            rule.Reward.MaxSelectionQuantityPerOrder = null;
        }

        private async Task SyncScopeItemsAsync(
            MarketingCampaignEditDto input,
            MarketingRule rule,
            long websiteId)
        {
            var requestedItems = input.ConditionType == MarketingConditionTypeEnum.OrderAmount
                ? new List<MarketingScopeItemEditDto>()
                : input.ScopeItems;

            var productIds = requestedItems
                .Select(x => x.TargetId)
                .Distinct()
                .ToList();

            if (productIds.Count > 0)
            {
                var validProductIds = await db.Prods
                    .Where(x => x.FK_WebsiteId == websiteId && productIds.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToListAsync();

                if (validProductIds.Count != productIds.Count)
                    throw new Exception("指定商品包含不存在、已刪除或不屬於目前網站的商品。");
            }

            var requestedKeys = requestedItems
                .Select(x => (x.TargetType, x.TargetId))
                .ToHashSet();

            foreach (var existing in rule.ScopeItems.Where(x => !x.IsDeleted))
            {
                if (!requestedKeys.Contains((existing.TargetType, existing.TargetId)))
                {
                    existing.IsDeleted = true;
                    existing.DeletionTime = DateTime.Now;
                }
            }

            foreach (var item in requestedItems)
            {
                var entity = rule.ScopeItems.FirstOrDefault(x =>
                    !x.IsDeleted &&
                    x.TargetType == item.TargetType &&
                    x.TargetId == item.TargetId);

                if (entity == null)
                {
                    entity = new MarketingScopeItem
                    {
                        MarketingRule = rule,
                        TargetType = item.TargetType,
                        TargetId = item.TargetId
                    };
                    rule.ScopeItems.Add(entity);
                }

                entity.RequiredQuantityPerQualification = 1;
            }
        }

        private async Task SyncRewardItemsAsync(
            MarketingCampaignEditDto input,
            MarketingRule rule,
            long websiteId)
        {
            var stockIds = input.RewardItems
                .Select(x => x.ProductStockId)
                .Distinct()
                .ToList();

            var validStockIds = await db.Prod_Stocks
                .Where(x => stockIds.Contains(x.Id) && x.Prod != null && x.Prod.FK_WebsiteId == websiteId)
                .Select(x => x.Id)
                .ToListAsync();

            if (validStockIds.Count != stockIds.Count)
                throw new Exception("優惠商品包含不存在、已刪除或不屬於目前網站的商品規格。");

            var requestedStockIds = stockIds.ToHashSet();
            SoftDeleteRewardItems(rule.Reward.Items.Where(x => !requestedStockIds.Contains(x.FK_ProdStockId)));

            foreach (var item in input.RewardItems)
            {
                var entity = rule.Reward.Items.FirstOrDefault(x =>
                    !x.IsDeleted && x.FK_ProdStockId == item.ProductStockId);

                if (entity == null)
                {
                    entity = new MarketingRewardItem
                    {
                        MarketingReward = rule.Reward,
                        FK_ProdStockId = item.ProductStockId
                    };
                    rule.Reward.Items.Add(entity);
                }

                entity.OfferPrice = item.OfferPrice;
                entity.MaxQuantityPerOrder = item.MaxQuantityPerOrder;
                entity.Enabled = item.Enabled;
                entity.SortOrder = item.SortOrder;
            }
        }

        private static void SoftDeleteScopeItems(IEnumerable<MarketingScopeItem> items)
        {
            foreach (var item in items.Where(x => !x.IsDeleted))
            {
                item.IsDeleted = true;
                item.DeletionTime = DateTime.Now;
            }
        }

        private static void SoftDeleteRewardItems(IEnumerable<MarketingRewardItem> items)
        {
            foreach (var item in items.Where(x => !x.IsDeleted))
            {
                item.IsDeleted = true;
                item.DeletionTime = DateTime.Now;
            }
        }

        private static string BuildStockName(Prod_Stock stock, IReadOnlyDictionary<long, string>? specNames = null)
        {
            var names = new[] { stock.FK_S1id, stock.FK_S2id }
                .Where(x => x.HasValue && x.Value > 0)
                .Select(x => specNames != null && specNames.TryGetValue(x!.Value, out var name) ? name : null)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
            if (names.Any())
                return string.Join(" / ", names);

            if (!string.IsNullOrWhiteSpace(stock.SpecDescription))
                return stock.SpecDescription;

            if (!string.IsNullOrWhiteSpace(stock.SubItemNo))
                return stock.SubItemNo;

            return "預設規格";
        }

        private static string? ValidateInput(MarketingCampaignEditDto input)
        {
            if (input == null)
                return "資料不可為空。";

            input.ScopeItems ??= new List<MarketingScopeItemEditDto>();
            input.RewardItems ??= new List<MarketingRewardItemEditDto>();

            if (string.IsNullOrWhiteSpace(input.Name))
                return "請輸入活動名稱。";

            if (!input.NeverEnd && input.EndTime == null)
                return "請設定活動結束時間。";

            if (!input.NeverEnd && input.EndTime <= input.StartTime)
                return "活動結束時間必須晚於開始時間。";

            if (input.RuleType == MarketingRuleTypeEnum.AmountDiscount)
            {
                if (input.MinAmount == null || input.MinAmount <= 0)
                    return "請輸入滿額門檻。";

                if (input.DiscountAmount == null || input.DiscountAmount <= 0)
                    return "請輸入折抵金額。";
            }

            if (input.RuleType == MarketingRuleTypeEnum.PercentDiscount)
            {
                if (input.MinAmount == null || input.MinAmount <= 0)
                    return "請輸入滿額門檻。";

                if (input.DiscountPercent == null || input.DiscountPercent <= 0 || input.DiscountPercent >= 100)
                    return "折扣百分比需大於 0 且小於 100。";
            }

            if (input.RuleType == MarketingRuleTypeEnum.AddOnPurchase)
            {
                var allowedConditions = new[]
                {
                    MarketingConditionTypeEnum.OrderAmount,
                    MarketingConditionTypeEnum.ScopeAmount,
                    MarketingConditionTypeEnum.ScopeQuantity,
                    MarketingConditionTypeEnum.BuySpecificProduct
                };

                if (!allowedConditions.Contains(input.ConditionType))
                    return "不支援此優惠商品資格條件。";

                if ((input.ConditionType == MarketingConditionTypeEnum.OrderAmount ||
                     input.ConditionType == MarketingConditionTypeEnum.ScopeAmount) &&
                    (input.MinAmount == null || input.MinAmount <= 0))
                    return "請輸入滿額門檻。";

                if ((input.ConditionType == MarketingConditionTypeEnum.ScopeQuantity ||
                     input.ConditionType == MarketingConditionTypeEnum.BuySpecificProduct) &&
                    (input.MinQuantity == null || input.MinQuantity <= 0))
                    return "請輸入指定商品合計購買件數。";

                if (input.ConditionType != MarketingConditionTypeEnum.OrderAmount && input.ScopeItems.Count == 0)
                    return "請至少選擇一項指定商品。";

                if (input.ScopeItems.Any(x =>
                        x.TargetType != MarketingScopeTargetTypeEnum.Product || x.TargetId <= 0))
                    return "指定商品資料格式錯誤。";

                if (input.ScopeItems
                    .GroupBy(x => new { x.TargetType, x.TargetId })
                    .Any(x => x.Count() > 1))
                    return "指定商品不可重複。";

                if (input.SelectionQuantityPerQualification <= 0)
                    return "每次資格可選數量必須大於 0。";

                if (input.RewardItems.Count == 0)
                    return "請至少設定一項加價購或贈品商品。";

                if (input.RewardItems.Any(x =>
                        x.ProductStockId <= 0 || x.OfferPrice < 0 || x.MaxQuantityPerOrder <= 0))
                    return "優惠商品的規格、活動價或每次資格可買件數格式錯誤。";

                if (input.RewardItems.Any(x =>
                        x.MaxQuantityPerOrder > input.SelectionQuantityPerQualification))
                    return "優惠商品的「每次資格可買」不可大於活動的「每次資格可選件數」。";

                if (input.RewardItems.GroupBy(x => x.ProductStockId).Any(x => x.Count() > 1))
                    return "相同商品規格不可重複設定。";
            }

            if (input.RuleType != MarketingRuleTypeEnum.AmountDiscount &&
                input.RuleType != MarketingRuleTypeEnum.PercentDiscount &&
                input.RuleType != MarketingRuleTypeEnum.AddOnPurchase)
                return "不支援此行銷規則。";

            return null;
        }
    }
}

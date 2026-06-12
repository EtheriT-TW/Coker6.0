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

        public MarketingAppService(CokerDbContext context, LoginUserData loginUserData, IMapper mapper)
        {
            db = context;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
        }

        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            var websiteId = await loginUserData.GetWebsiteId();
            var now = DateTime.Now;

            var query = db.MarketingCampaigns
                .Where(x => x.FK_WebsiteId == websiteId)
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

                    MinAmount = x.Rules
                        .Where(r => r.Enabled)
                        .OrderBy(r => r.SortOrder)
                        .Select(r => r.Condition.MinAmount)
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
                        .FirstOrDefault()
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
            dto.MinAmount = rule?.Condition?.MinAmount;
            dto.DiscountAmount = rule?.Reward?.DiscountAmount;
            dto.DiscountPercent = rule?.Reward?.DiscountPercent;
            dto.MaxDiscountAmount = rule?.Reward?.MaxDiscountAmount;

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

                MarketingCampaign? campaign;

                if (input.Id > 0)
                {
                    campaign = await db.MarketingCampaigns
                        .Include(x => x.Rules)
                            .ThenInclude(x => x.Condition)
                        .Include(x => x.Rules)
                            .ThenInclude(x => x.Reward)
                        .FirstOrDefaultAsync(x => x.Id == input.Id && x.FK_WebsiteId == websiteId);

                    if (campaign == null)
                    {
                        throw new Exception("找不到行銷活動資料，或您沒有權限編輯此活動。");
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
                rule.ScopeType = MarketingScopeTypeEnum.AllOrder;
                rule.Enabled = true;

                if (rule.Condition == null)
                {
                    rule.Condition = new MarketingCondition
                    {
                        MarketingRule = rule
                    };
                }
                rule.Condition.ConditionType = MarketingConditionTypeEnum.OrderAmount;
                rule.Condition.MinAmount = input.MinAmount;
                rule.Condition.MinQuantity = null;
                rule.Condition.OnlyScopeItems = false;
                rule.Condition.ExcludeDiscountedItems = false;

                if (rule.Reward == null)
                {
                    rule.Reward = new MarketingReward
                    {
                        MarketingRule = rule
                    };
                }
                rule.Reward.DeliveryType = MarketingRewardDeliveryTypeEnum.ApplyImmediately;
                rule.Reward.DiscountAmount = input.RuleType == MarketingRuleTypeEnum.AmountDiscount
                    ? input.DiscountAmount
                    : null;
                rule.Reward.DiscountPercent = input.RuleType == MarketingRuleTypeEnum.PercentDiscount
                    ? input.DiscountPercent
                    : null;
                rule.Reward.MaxDiscountAmount = input.RuleType == MarketingRuleTypeEnum.PercentDiscount
                    ? input.MaxDiscountAmount
                    : null;

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
                        .ThenInclude(x => x.Reward)
                    .Include(x => x.Rules)
                        .ThenInclude(x => x.ScopeItems)
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
                    new() { Value = (int)MarketingRuleTypeEnum.PercentDiscount, Text = "滿額打折" }
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

                var output = new CartMarketingCampaignsDto();

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
                        continue;

                    output.OrderDiscounts.Add(new CartMarketingCampaignDto
                    {
                        Id = campaign.Id,
                        Name = campaign.Name,
                        CampaignType = campaign.CampaignType,
                        Priority = campaign.Priority,
                        CanStack = campaign.CanStack,
                        Repeatable = campaign.Repeatable,
                        Rules = rules
                    });
                }

                return new ResponseMessageDto
                {
                    Success = true,
                    Object = output
                };
            }
            catch (Exception ex)
            {
                return new ResponseMessageDto
                {
                    Success = false,
                    Message = "取得購物車行銷活動發生錯誤。",
                    Error = ex.Message
                };
            }
        }

        private static string? ValidateInput(MarketingCampaignEditDto input)
        {
            if (input == null)
                return "資料不可為空。";

            if (string.IsNullOrWhiteSpace(input.Name))
                return "請輸入活動名稱。";

            if (!input.NeverEnd && input.EndTime == null)
                return "請設定活動結束時間。";

            if (!input.NeverEnd && input.EndTime <= input.StartTime)
                return "活動結束時間必須晚於開始時間。";

            if (input.MinAmount == null || input.MinAmount <= 0)
                return "請輸入滿額門檻。";

            if (input.RuleType == MarketingRuleTypeEnum.AmountDiscount)
            {
                if (input.DiscountAmount == null || input.DiscountAmount <= 0)
                    return "請輸入折抵金額。";
            }

            if (input.RuleType == MarketingRuleTypeEnum.PercentDiscount)
            {
                if (input.DiscountPercent == null || input.DiscountPercent <= 0 || input.DiscountPercent >= 100)
                    return "折扣百分比需大於 0 且小於 100。";
            }

            return null;
        }
    }
}
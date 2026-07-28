using AutoMapper;
using DevExtreme.AspNet.Data;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Dto.AuditLog;
using EtheriT.Coker.Application.Shared.Dto.Remote;
using EtheriT.Coker.Application.Shared.Remote;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DevExtreme.AspNet.Mvc;
using System.Collections;
using System.Data;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Core.Models;
using Microsoft.Data.SqlClient;

namespace EtheriT.Coker.Application.Remote
{
    public class RemoteAppService : IRemoteAppService
    {
        private const int MinimumVisibleSeconds = 2;
        private const int EngagementSeconds = 10;
        private const int MaximumTrackedSeconds = 5 * 60;

        private static readonly string[] BotKeywords =
        {
            "googlebot", "bingbot", "baiduspider", "yandexbot", "duckduckbot",
            "applebot", "petalbot", "semrushbot", "ahrefsbot", "dotbot",
            "mj12bot", "gptbot", "oai-searchbot", "claudebot", "perplexitybot",
            "bytespider", "facebookexternalhit", "discordbot", "telegrambot",
            "slackbot", "uptimerobot", "crawler", "slurp", "headlesschrome",
            "phantomjs", "python-requests", "curl/", "wget/"
        };

        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly ITokenAppService tokenAppService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;
        public RemoteAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            ITokenAppService tokenAppService,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor
        )
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.httpContextAccessor = httpContextAccessor;
            this.tokenAppService = tokenAppService;
        }

        public async Task CollectRemoteTracking(RemoteInputDto page, RemoteTrackingCollectDto tracking)
        {
            if (tracking.EventId == Guid.Empty || tracking.VisibleSeconds < MinimumVisibleSeconds)
                return;

            var browserInfo = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
            if (IsKnownBot(browserInfo))
                return;

            var visibleSeconds = Math.Clamp(
                tracking.VisibleSeconds,
                MinimumVisibleSeconds,
                MaximumTrackedSeconds);

            var remote = await db.Remotes
                .SingleOrDefaultAsync(e => e.TrackingEventId == tracking.EventId);

            if (remote == null)
            {
                remote = mapper.Map<Core.Models.Remote>(page);
                remote.TrackingEventId = tracking.EventId;
                remote.BrowserInfo = browserInfo;
                remote.ClientIpAddress = loginUserData.GetClientIP();
                remote.UUID = await tokenAppService.GetUUID();
                remote.ExecutionTime = DateTime.Now;
                remote.LeaveTime = remote.ExecutionTime;
                remote.LastHeartbeatAt = remote.ExecutionTime;
                remote.State = RemoteStateEnum.資料不完整;
                remote.TrafficQuality = RemoteTrafficQualityEnum.前端已確認;
                remote.IsEngaged = false;
                remote.TimeOnPage = visibleSeconds;
                remote.HasInteraction = tracking.HasInteraction;
                db.Remotes.Add(remote);

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
                {
                    db.ChangeTracker.Clear();
                    remote = await db.Remotes
                        .SingleOrDefaultAsync(e => e.TrackingEventId == tracking.EventId);
                    if (remote == null)
                        throw;
                }
            }

            if (!MatchesPage(remote, page))
                return;

            var now = DateTime.Now;
            var becameEngaged = remote.IsEngaged != true
                && (visibleSeconds >= EngagementSeconds || tracking.HasInteraction);

            remote.TimeOnPage = Math.Max(remote.TimeOnPage, visibleSeconds);
            remote.LeaveTime = now;
            remote.LastHeartbeatAt = now;
            remote.HasInteraction = remote.HasInteraction == true || tracking.HasInteraction;

            if (becameEngaged)
            {
                remote.IsEngaged = true;
                remote.EngagedAt = now;
                remote.State = RemoteStateEnum.未處理;
                remote.TrafficQuality = RemoteTrafficQualityEnum.有效互動;
                await AddActivityTagsAsync(remote);
            }

            await db.SaveChangesAsync();
        }

        private static bool IsKnownBot(string? browserInfo)
        {
            if (string.IsNullOrWhiteSpace(browserInfo))
                return true;

            var normalized = browserInfo.ToLowerInvariant();
            return BotKeywords.Any(normalized.Contains);
        }

        private static bool MatchesPage(Core.Models.Remote remote, RemoteInputDto page)
        {
            return remote.FK_WebsiteId == page.FK_WebsiteId
                && remote.FK_WebmenuId == page.FK_WebmenuId
                && remote.FK_ArticleId == page.FK_ArticleId
                && remote.FK_ProdId == page.FK_ProdId
                && remote.FK_TechCertId == page.FK_TechCertId;
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            return exception.InnerException is SqlException sqlException
                && (sqlException.Number == 2601 || sqlException.Number == 2627);
        }

        private async Task AddActivityTagsAsync(Core.Models.Remote remote)
        {
            IQueryable<Tag_Associate> tagQuery;
            if (remote.FK_ProdId.HasValue)
            {
                tagQuery = db.Tag_Associates.Where(e =>
                    e.FK_AId == remote.FK_ProdId.Value
                    && e.Type == TagAssociateTypeEnum.商品);
            }
            else if (remote.FK_ArticleId.HasValue)
            {
                tagQuery = db.Tag_Associates.Where(e =>
                    e.FK_AId == remote.FK_ArticleId.Value
                    && e.Type == TagAssociateTypeEnum.文章);
            }
            else
            {
                return;
            }

            var tagIds = await tagQuery.Select(e => e.FK_TId).Distinct().ToListAsync();
            if (tagIds.Count == 0)
                return;

            var timeOnPageMinutes = remote.TimeOnPage / 60.0;
            var tags = new List<UserActivityTags>();

            foreach (var tagId in tagIds)
            {
                var lastActivityTime = await db.UserActivityTags
                    .AsNoTracking()
                    .Where(e => e.FK_TId == tagId && e.Remote.UUID == remote.UUID)
                    .OrderByDescending(e => e.CreateTime)
                    .Select(e => (DateTime?)e.CreateTime)
                    .FirstOrDefaultAsync();

                var timeDecayFactor = lastActivityTime.HasValue
                    ? Math.Exp(-0.1 * (DateTime.Now - lastActivityTime.Value).TotalDays)
                    : 0.1;

                tags.Add(new UserActivityTags
                {
                    FK_TId = tagId,
                    FK_RemoteId = remote.Id,
                    Weight = (float)(0.5 * Math.Pow(1 + timeDecayFactor, timeOnPageMinutes))
                });
            }

            db.UserActivityTags.AddRange(tags);
        }
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            var siteId = await loginUserData.GetWebsiteId();
            var result =
                from statistic in db.RemoteDailyStatistics.AsNoTracking()
                where statistic.FK_WebsiteId == siteId
                   && statistic.Scope == 1
                join articleData in db.Article.IgnoreQueryFilters().AsNoTracking()
                    on statistic.FK_ArticleId equals articleData.Id into articles
                from article in articles.DefaultIfEmpty()
                join productData in db.Prods.IgnoreQueryFilters().AsNoTracking()
                    on statistic.FK_ProdId equals productData.Id into products
                from product in products.DefaultIfEmpty()
                join menuData in db.WebMenus.IgnoreQueryFilters().AsNoTracking()
                    on statistic.FK_WebmenuId equals menuData.Id into menus
                from menu in menus.DefaultIfEmpty()
                join certificateData in db.TechnicalCertificates.IgnoreQueryFilters().AsNoTracking()
                    on statistic.FK_TechCertId equals certificateData.Id into certificates
                from certificate in certificates.DefaultIfEmpty()
                select new RemoteListOtputDto
                {
                    date = statistic.StatisticDate,
                    type = statistic.FK_ArticleId > 0 ? "文章" :
                           statistic.FK_ProdId > 0 ? "商品" :
                           statistic.FK_TechCertId > 0 ? "標章認證" :
                           statistic.FK_WebmenuId > 0 ? "選單" : "其他",
                    name = article != null ? article.Title ?? "未命名文章" :
                           product != null ? product.Title :
                           certificate != null ? certificate.Title :
                           menu != null ? menu.Title ?? "未命名選單" : "已刪除內容",
                    count = statistic.EffectiveViews,
                    MemCount = statistic.EffectiveUniqueVisitors,
                    TotalTimeOnPagePerTime = statistic.EffectiveUniqueVisitors > 0
                        ? (double)statistic.TotalVisibleSeconds / statistic.EffectiveUniqueVisitors
                        : 0
                };

            if (loadOptions.Sort == null)
            {
                var Sort = new List<SortingInfo>
                {
                    new SortingInfo { Selector = "date", Desc = true },
                    new SortingInfo { Selector = "count", Desc = true }
                };
                loadOptions.Sort = Sort.ToArray();
            }

            var output = await DataSourceLoader.LoadAsync(result, loadOptions);
            return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        //從資料庫撈使用者紀錄
        public async Task<JsonResult> GetPageList(DataSourceLoadOptions loadOptions)
        {
            var siteId = await loginUserData.GetWebsiteId();
            var query = db.RemoteDailyStatistics
                .AsNoTracking()
                .Where(statistic =>
                    statistic.FK_WebsiteId == siteId
                    && statistic.Scope == 0)
                .Select(statistic => new RemoteListOtputDto
                {
                    date = statistic.StatisticDate,
                    count = statistic.EffectiveViews,
                    MemCount = statistic.EffectiveUniqueVisitors
                });
            if (loadOptions.Sort == null)
            {
                var Sort = new List<SortingInfo>{new SortingInfo
                    {
                        Selector = "date",
                        Desc = true
                    } };
                loadOptions.Sort = Sort.ToArray();
            }
            var output = await DataSourceLoader.LoadAsync(query, loadOptions);
            //取日期跟時間
            return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }

        public async Task<ResponseMessageDto> GetRemoteCount(GetRemoteCountInputDto dto)
        {
            var siteId = await loginUserData.GetWebsiteId();
            var data = await db.RemoteDailyStatistics
                .AsNoTracking()
                .Where(statistic =>
                    statistic.FK_WebsiteId == siteId
                    && statistic.Scope == 0
                    && statistic.StatisticDate >= dto.StartDate.Date
                    && statistic.StatisticDate < dto.EndDate)
                .OrderBy(statistic => statistic.StatisticDate)
                .Select(statistic => new RemoteListOtputDto
                {
                    date = statistic.StatisticDate,
                    count = statistic.EffectiveViews,
                    MemCount = statistic.EffectiveUniqueVisitors
                })
                .ToListAsync();

            return new ResponseMessageDto
            {
                Success = true,
                Object = new GetRemoteCountOutputDto
                {
                    remoteListOtputDtos = data
                }
            };
        }
        public async Task<ResponseMessageDto> GetTotalRemoteCount()
        {
            try
            {
                var siteId = await loginUserData.GetWebsiteId();
                var siteStatistics = db.RemoteDailyStatistics
                    .AsNoTracking()
                    .Where(statistic =>
                        statistic.FK_WebsiteId == siteId
                        && statistic.Scope == 0);

                var result = await siteStatistics
                    .GroupBy(_ => 1)
                    .Select(group => new
                    {
                        FirstDate = group.Min(statistic => statistic.StatisticDate),
                        AllCount = group.Sum(statistic => statistic.EffectiveViews),
                        AllMemCount = group.Sum(statistic => statistic.EffectiveUniqueVisitors)
                    })
                    .FirstOrDefaultAsync();

                var output = new GetRemoteCountOutputDto
                {
                    remoteListOtputDtos = new List<RemoteListOtputDto> { new RemoteListOtputDto
                    {
                        date = result?.FirstDate ?? DateTime.Today,
                        count = 0,
                        MemCount = 0
                    }},
                    AllCount = result?.AllCount ?? 0,
                    AllMemCount = result?.AllMemCount ?? 0
                };
                return new ResponseMessageDto
                {
                    Object = output,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseMessageDto
                {
                    Error = ex.Message,
                    Success = false
                };
            }
        }
    };
}

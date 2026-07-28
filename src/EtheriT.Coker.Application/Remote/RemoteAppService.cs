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
            long siteId = await loginUserData.GetWebsiteId();
            var query = from r in db.Remotes.AsNoTracking()
                        where r.FK_WebsiteId == siteId
                        join a in db.Article.Where(e => !e.IsDeleted) on r.FK_ArticleId equals a.Id into articles
                        from article in articles.DefaultIfEmpty()
                        join p in db.Prods.Where(e => !e.IsDeleted) on r.FK_ProdId equals p.Id into products
                        from product in products.DefaultIfEmpty()
                        join m in db.WebMenus.Where(e => !e.IsDeleted) on r.FK_WebmenuId equals m.Id into menus
                        from menu in menus.DefaultIfEmpty()
                        join t in db.TechnicalCertificates.Where(e => !e.IsDeleted) on r.FK_TechCertId equals t.Id into certs
                        from cert in certs.DefaultIfEmpty()
                        select new
                        {
                            r.ExecutionTime.Date,
                            PageType = article != null ? "文章" :
                                       product != null ? "商品" :
                                       cert != null ? "標章認證" :
                                       menu != null ? "選單" : "其他",
                            Title = article != null ? article.Title :
                                    product != null ? product.Title :
                                    cert != null ? cert.Title :
                                    menu != null ? menu.Title : "其他",
                            UserIdentifier = r.FK_UserId.HasValue
                                ? "user:" + r.FK_UserId.Value.ToString()
                                : r.UUID != Guid.Empty
                                    ? "uuid:" + r.UUID.ToString()
                                    : "ip:" + (r.ClientIpAddress ?? string.Empty),
                            r.TimeOnPage
                        };

            // 保持 IQueryable，讓分組、統計、排序及分頁都在資料庫執行。
            var statistics = query
                .GroupBy(r => new { r.Date, r.PageType, r.Title })
                .Select(g => new
                {
                    date = g.Key.Date,
                    type = g.Key.PageType,
                    name = g.Key.Title,
                    count = g.LongCount(),
                    MemCount = g.Select(r => r.UserIdentifier).Distinct().LongCount(),
                    TotalTimeOnPage = g.Sum(r => (long)r.TimeOnPage)
                });

            var result = statistics.Select(s => new RemoteListOtputDto
                {
                    date = s.date,
                    type = s.type,
                    name = s.name,
                    count = s.count,
                    MemCount = s.MemCount,
                    TotalTimeOnPagePerTime = s.MemCount > 0
                        ? (double)s.TotalTimeOnPage / s.MemCount
                        : 0
                });

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
            long siteId = await loginUserData.GetWebsiteId();
            var query = db.Remotes
                    .AsNoTracking()
                    .Where(e => e.FK_WebsiteId == siteId)
                    .Join(db.WebMenus.AsNoTracking().Where(e => e.FK_WebsiteId == siteId && !e.IsDeleted),
                          d => d.FK_WebmenuId,
                          m => m.Id,
                          (d, m) => new
                          {
                              d.ExecutionTime.Date,
                              UserIdentifier = d.FK_UserId.HasValue
                                  ? "user:" + d.FK_UserId.Value.ToString()
                                  : d.UUID != Guid.Empty
                                      ? "uuid:" + d.UUID.ToString()
                                      : "ip:" + (d.ClientIpAddress ?? string.Empty)
                          })
                    .GroupBy(d => d.Date)
                    .Select(g => new RemoteListOtputDto
                    {
                        date = g.Key.Date,
                        count = g.LongCount(),   // 人次
                        MemCount = g.Select(d => d.UserIdentifier).Distinct().LongCount() // 人數
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
            ResponseMessageDto response = new ResponseMessageDto();
            long siteId = await loginUserData.GetWebsiteId();
            var data =
                from d in db.Remotes //使用者瀏覽紀錄
                join m in db.WebMenus.Where(e => e.FK_WebsiteId == siteId && !e.IsDeleted) on d.FK_WebmenuId equals m.Id
                where d.ExecutionTime.Date >= dto.StartDate && d.ExecutionTime.Date < dto.EndDate

                group d by new
                {
                    d.ExecutionTime.Date,
                    d.ClientIpAddress
                } into g
                select new
                {
                    g.Key.Date,
                    count = g.Count(),
                };
            if (data != null)
            {
                var dataQuery = from d in data
                                group d by new
                                {
                                    d.Date,
                                } into d
                                select new RemoteListOtputDto
                                {
                                    date = d.Key.Date,//時間
                                    count = d.Where(e => e.Date == d.Key.Date).Sum(e => e.count),//人次
                                    MemCount = d.Count(),  //人數
                                };
                response.Object = new GetRemoteCountOutputDto
                {
                    remoteListOtputDtos = await dataQuery.ToListAsync()
                };
                response.Success = true;
                //取日期跟時間
                return response;
            }
            else throw new Exception("查無資料");
        }
        public async Task<ResponseMessageDto> GetTotalRemoteCount()
        {
            ResponseMessageDto response = new ResponseMessageDto();
            try
            {
                long siteId = await loginUserData.GetWebsiteId();
                var first = db.Remotes.Include(r => r.WebMenu)
                    .Where(r => r.WebMenu.FK_WebsiteId == siteId)
                    .OrderBy(r => r.ExecutionTime).FirstOrDefault();

                var result = db.Remotes.Include(r => r.WebMenu)
                    .Where(r => r.WebMenu.FK_WebsiteId == siteId)
                    .GroupBy(r => new
                    {
                        r.ExecutionTime.Date,
                        UserIdentifier = r.UUID == Guid.Empty ? r.ClientIpAddress : r.UUID.ToString()
                    })
                    .Select(g => new
                    {
                        g.Key.UserIdentifier,
                        VisitCount = g.Count()  // 當日每個使用者的瀏覽次數
                    }).GroupBy(r => 1)  // 將所有結果分組為一個組
                    .Select(g => new
                    {
                        AllCount = g.Sum(r => r.VisitCount),  // 總瀏覽人次
                        AllMemCount = g.Count()  // 總人數
                    })
                    .FirstOrDefault();  // 取得最終統計結果
                var output = new GetRemoteCountOutputDto
                {
                    remoteListOtputDtos = new List<RemoteListOtputDto> { new RemoteListOtputDto
                    {
                        date = first == null ? DateTime.Now : first.ExecutionTime,
                        count = 0,
                        MemCount = 0
                    }},
                    AllCount = result?.AllCount??0,  // 總瀏覽人次
                    AllMemCount = result?.AllMemCount ?? 0 // 總人數
                };
                response.Object = output;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }
            return response;
        }
    };
}

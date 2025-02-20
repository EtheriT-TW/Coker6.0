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
using Microsoft.Extensions.Caching.Memory;
using EtheriT.Coker.Application.Shared.Dto.UserHabits;
using static Azure.Core.HttpHeader;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EtheriT.Coker.Application.Remote
{
	public class RemoteAppService: IRemoteAppService
	{
		private readonly CokerDbContext db;
		private readonly LoginUserData loginUserData;
        private readonly StringHandler stringHandler;
        private readonly ITokenAppService tokenAppService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMemoryCache memoryCache;
        private readonly IMapper mapper;
		public RemoteAppService(
            CokerDbContext db, 
            LoginUserData loginUserData,
            StringHandler stringHandler,
            ITokenAppService tokenAppService, 
            IMapper mapper,
            IMemoryCache memoryCache,
            IHttpContextAccessor httpContextAccessor
        ) { 
			this.db = db;
			this.loginUserData = loginUserData;
            this.stringHandler = stringHandler;
			this.mapper = mapper;
			this.httpContextAccessor = httpContextAccessor;
            this.tokenAppService = tokenAppService;
            this.memoryCache = memoryCache;
		}
		public async Task<ResponseMessageDto> insertRemote(RemoteInputDto dto) {
			ResponseMessageDto response= new ResponseMessageDto();
			try {
				Core.Models.Remote r = mapper.Map<Core.Models.Remote>(dto);
                if (httpContextAccessor.HttpContext != null)
					r.BrowserInfo = httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();
                List<string> botKeywords = new List<string> { 
                    "Googlebot", "Bingbot", "AhrefsBot", "ImagesiftBot", "DotBot", "SemrushBot", "PetalBot", "OAI-SearchBot", 
                    "Applebot", "CCBot", "MJ12bot", "AdsBot-Google", "Slurp","perplexitybot", "coccocbot","https://openai.com/bot","GPTBot",
                    "YandexBot","Google-Read-Aloud","DataForSeoBot","ClaudeBot", "facebookexternalhit", "line-poker", "UptimeRobot","ZoominfoBot",
                    "KStandBot","ZoominfoBot","reurl-bot","BacklinksExtendedBot","serpstatbot","Qwantbot","Slackbot","SMTBot","aiHitBot","BLEXBot",
                    "TelegramBot","trendictionbot","INETDEX-BOT","Spider_Bot","msnbot","Facebot","http://yandex.com/bots","2ip bot","SpringserveBot",
                    "DuckDuckBot","wpbot","SurdotlyBot","Discordbot","bot.html","bitlybot","adsbot.html","WellKnownBot","Orbbot","Timpibot","YodaoBot",
                    "org_bot","AliyunSecBot","RU_Bot","/bot/","/bots","/robots","BitSightBot","MixrankBot","StorygizeBot","StorygizeBot","Dcard-link-preview-bot",
                    "Baidu-YunGuanCe-Bot","domainsbot","robot","Bravebot","DuckDuckGo-Favicons-Bot","Sansanbot"
                };
                if (string.IsNullOrEmpty(r.BrowserInfo) || botKeywords.Any(bot => r.BrowserInfo.Contains(bot))) { 
                    throw new Exception("不接受機器人訪問");
                }
				r.ClientIpAddress = loginUserData.GetClientIP();
                r.UUID = await tokenAppService.GetUUID();
                db.Add(r);
				await db.SaveChangesAsync();
                string PageKey = stringHandler.RandonCode(RandomStringType.數字加英文大小寫 ,8);
                memoryCache.Set($"RemoteId-{PageKey}-{r.UUID}", r.Id, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1), // 最多存活 1 小時
                    SlidingExpiration = TimeSpan.FromMinutes(10) // 每次訪問後延長 10 分鐘
                });
                response.Message = PageKey;
                response.Success = true;
			}catch (Exception ex)
			{
				response.Error = ex.Message;
			}
			return response;
		}
		public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions) {
			long siteId =  await loginUserData.GetWebsiteId();
            var query = from r in db.Remotes
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
                            r.FK_UserId,
                            r.UUID,
                            r.TimeOnPage
                        };

            // **讓 EF Core 先查詢所有數據**
            var rawData = await query.ToListAsync();

            // **記憶體中分組 & 計算**
            var result = rawData
                .GroupBy(r => new { r.Date, r.PageType, r.Title })
                .Select(g =>
                {
                    int uniqueUserCount = g.Select(r => r.FK_UserId)
                                           .Concat(g.Select(r => (long?)r.UUID.GetHashCode()))
                                           .Distinct()
                                           .Count();  // **計算不重複的用戶數**

                    return new RemoteListOtputDto
                    {
                        date = g.Key.Date,
                        type = g.Key.PageType,
                        name = g.Key.Title,
                        count = g.Count(),  // **總瀏覽次數**
                        MemCount = uniqueUserCount,  // **不重複的用戶數**
                        TotalTimeOnPagePerTime = uniqueUserCount > 0
                            ? (double)g.Sum(r => r.TimeOnPage) / uniqueUserCount  // **每個人平均停留時間**
                            : 0  // **避免除以 0**
                    };
                })
                .ToList();

            if (loadOptions.Sort == null)
            {
                var Sort = new List<SortingInfo>
                {
                    new SortingInfo { Selector = "date", Desc = true },
                    new SortingInfo { Selector = "count", Desc = true }
                };
                loadOptions.Sort = Sort.ToArray();
            }

            var output = DataSourceLoader.Load(result, loadOptions);
            return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        //從資料庫撈使用者紀錄
        public async Task<JsonResult> GetPageList(DataSourceLoadOptions loadOptions) {
			long siteId = await loginUserData.GetWebsiteId();
            var query = db.Remotes
                    .Where(e => e.FK_WebsiteId == siteId)
                    .Join(db.WebMenus.Where(e => e.FK_WebsiteId == siteId && !e.IsDeleted),
                          d => d.FK_WebmenuId,
                          m => m.Id,
                          (d, m) => new {
                              d.ExecutionTime.Date,
                              UserIdentifier = d.UUID == Guid.Empty ? d.ClientIpAddress : d.UUID.ToString()
                          })
                    .GroupBy(d => d.Date)
                    .Select(g => new RemoteListOtputDto
                    {
                        date = g.Key.Date,
                        count = g.Count(),   // 人次
                        MemCount = g.Select(d => d.UserIdentifier).Distinct().Count() // 人數
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
            var output = DataSourceLoader.Load(query, loadOptions);
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
        public async Task UpdateRemoteTime(SetTrackTimeDto dto) {
            Guid UUID = await tokenAppService.GetUUID();
            long id = memoryCache.Get<long>($"RemoteId-{dto.PageKey}-{UUID}");
            int maxspan = 5 * 60;
            if (id != 0) {
                var remote = await db.Remotes.Where(e => e.Id == id).FirstOrDefaultAsync();
                if (remote != null) {
                    remote.LeaveTime = DateTime.Now;
                    remote.TimeOnPage += (int)Math.Round(dto.TimeSpan / 1000.0);
                    remote.TimeOnPage = Math.Min(remote.TimeOnPage, maxspan);
                    remote.State = RemoteStateEnum.未處理;
                    remote.UUID = UUID;
                    db.SaveChanges();

                    List<UserActivityTags>? tags = new List<UserActivityTags>();
                    double TimeOnPage = remote.TimeOnPage / 60.0;
                    if (!remote.FK_ProdId.IsNullOrEmpty())
                    {
                        tags = (from t in db.Tag_Associates.Where(e => e.FK_AId == remote.FK_ProdId && e.Type == TagAssociateTypeEnum.商品)
                                select new UserActivityTags
                                {
                                    FK_TId = t.FK_TId,
                                    FK_RemoteId = id,
                                    Weight = (float)(0.5 * Math.Pow(1 + 0.1, TimeOnPage))
                                }).ToList();
                    }
                    else if (!remote.FK_ArticleId.IsNullOrEmpty()) {
                        tags = (from t in db.Tag_Associates.Where(e => e.FK_AId == remote.FK_ArticleId && e.Type == TagAssociateTypeEnum.文章)
                                select new UserActivityTags
                                {
                                    FK_TId = t.FK_TId,
                                    FK_RemoteId = id,
                                    Weight = (float)(0.5 * Math.Pow(1 + 0.1, TimeOnPage))
                                }).ToList();
                    }
                    if (tags.Any()) {
                        List<UserActivityTags> AddUserActivityTags = new List<UserActivityTags>();
                        tags.ForEach(e => {
                            double daysSinceLastInteraction = 0;
                            var last = db.UserActivityTags.Include(u => u.Remote).Where(u => u.FK_TId == e.FK_TId && u.Remote.UUID == UUID).OrderByDescending(u => u.CreateTime).FirstOrDefault();
                            if (last != null)
                            {
                                daysSinceLastInteraction = (DateTime.Now - last.CreateTime).TotalDays;
                                double timeDecayFactor = Math.Exp(-0.1 * daysSinceLastInteraction);
                                e.Weight = (float)(0.5 * Math.Pow(1 + timeDecayFactor, TimeOnPage));
                            }
                            var item = db.UserActivityTags.Where(u => u.FK_RemoteId == e.FK_RemoteId && u.FK_TId == e.FK_TId).FirstOrDefault();
                            if (item != null)
                            {
                                item.Weight = e.Weight;
                            }
                            else AddUserActivityTags.Add(e);
                        });
                        db.UserActivityTags.AddRange(AddUserActivityTags);
                        db.SaveChanges();
                    }
                }
            }
            
        }
    };
}

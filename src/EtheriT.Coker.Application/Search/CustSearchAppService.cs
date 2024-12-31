using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.Dto.Article;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using EtheriT.Coker.Application.Shared.Dto.Search;
using DevExtreme.AspNet.Data;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Tag;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.Shared.i18n;
using EtheriT.Coker.Core.Models;
using Newtonsoft.Json.Linq;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Token;

namespace EtheriT.Coker.Application.Search
{
    public class CustSearchAppService: ICustSearchAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IMapper mapper;
        private readonly ITagAppService tagAppService;
        private readonly IWebMenuApplication webMenuApplicationService;
        private readonly ITokenAppService tokenAppService;
        public CustSearchAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IMapper mapper,
            ITagAppService tagAppService,
            IWebMenuApplication webMenuApplicationService,
            ITokenAppService tokenAppService
        ) {
            this.db = db;
            this.loginUserData = loginUserData;
            this.mapper = mapper;
            this.tagAppService = tagAppService;
            this.webMenuApplicationService = webMenuApplicationService;
            this.tokenAppService = tokenAppService;
        }
        public async Task<JsonResult> GetAll(DataSourceLoadOptions loadOptions) {
            try
            {
                long WebsiteID = await loginUserData.GetWebsiteId();
                var result = db.CustSearch;

                if (result != null)
                {
                    var dataQuery = from e in result
                                    where !e.IsDeleted && e.FK_WebsiteId == WebsiteID
                                    select new CuseSearchListDto
                                    {
                                        Id = e.Id,
                                        Title = e.Title,
                                        Visible = e.Visible,
                                        SerNO =e.SerNo
                                    };
                    var output = await DataSourceLoader.LoadAsync(dataQuery, loadOptions);
                    return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
                }
                else throw new Exception("查無目錄資料");
            }
            catch (Exception e)
            {

            }

            return new JsonResult(new List<CuseSearchListDto>(), new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
        }
        public async Task<List<SearchItemDto>> GetSearchList(long sid)
        {
            var site = await db.Websites.Where(e => !e.IsDeleted).Where(e => e.Id == sid).FirstOrDefaultAsync();
            List<SearchItemDto> list = new List<SearchItemDto> ();
            if (site != null)
            {
				switch (site.OrgName)
				{
					case "ksp":
						list.Add(
							new SearchItemDto
							{
								Id = 1,
								Name = "找廠商"
							}
						);
						break;
					case "eplus":
						list.Add(
							new SearchItemDto
							{
								Id = 2,
								Name = "最新消息"
							}
						);
						break;
				}
                if ((int)site.Level >= 2)
                {
                    bool hasProds = await db.Prods.Where(e => e.FK_WebsiteId == site.Id).Where(e => !e.IsDeleted).Where(e => !e.RemovedFromShelves).AnyAsync();
                    if (hasProds) {
                        list.Add(
                            new SearchItemDto
                            {
                                Id = 3,
                                Name = L.get("FindProduct")
                            }
                        );
                    }
                }
            }
            list.Add(new SearchItemDto
            {
                Id = 0,
                Name = L.get("FindArticle")
            });
            return list;
        }

        public async Task SaveSearchLog(SaveSearchLogDto dto)
        {
            string Ip = loginUserData.GetClientIP()??"";
            long WebsiteID = dto.FK_WebsiteId == 0 ? await loginUserData.GetWebsiteId() : dto.FK_WebsiteId;
            Guid uuid = await tokenAppService.GetUUID();
            SearchLog log = new SearchLog
            {
                Key = dto.Key,
                ClientIpAddress = Ip,
                FK_WebsiteId = WebsiteID,
                FK_CustSearchId = dto.FK_CustSearchId,
                UUID = uuid
            };
            var reg = db.SearchLogs.Where(e => e.CreationTime.Date == log.CreationTime.Date && e.FK_WebsiteId == log.FK_WebsiteId && e.FK_CustSearchId == log.FK_CustSearchId)
                        .Where(e => e.Key == log.Key && e.UUID == log.UUID);
            if (!reg.Any())
            {
                db.SearchLogs.Add(log);
                await db.SaveChangesAsync();
            }
        }
        public async Task<ResponseMessageDto> GetSearchKeyList(long websiteId) {
            ResponseMessageDto response = new ResponseMessageDto();
            var minTimes = 10;
            var maxRecords = 10000;
            try
            {
                var resultQuery = db.SearchLogs
                    .Where(log => log.FK_WebsiteId == websiteId)
                    .Where(log => !log.Key.All(char.IsDigit) &&
                                  !log.Key.All(ch => char.IsSymbol(ch) || char.IsPunctuation(ch)))
                    .OrderByDescending(log => log.CreationTime)
                    .Take(maxRecords);

                var lastInsertTime = (await resultQuery.MaxAsync(log => log.CreationTime)).Date;

                var result = await resultQuery
                    .GroupBy(log => log.Key.ToUpper())
                    .Select(group => new SearchKeyDto{
                        Key = group.Key,
                        Times = group.Count()
                    })
                    .Where(entry => entry.Times > minTimes)
                    .OrderByDescending(entry => entry.Times)
                    .ThenBy(entry => entry.Key)
                    .Take(1000)
                    .ToListAsync();
                response.Object = new SearchKeyListDto {
                    Keys = result,
                    LastInsertTime = lastInsertTime,
                };
                response.Success = true;
            }
            catch(Exception ex) { 
                response.Error = ex.Message;
            }
            return response;
        }
    }
}

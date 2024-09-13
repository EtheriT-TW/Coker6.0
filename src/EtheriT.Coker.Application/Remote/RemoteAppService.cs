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

namespace EtheriT.Coker.Application.Remote
{
	public class RemoteAppService: IRemoteAppService
	{
		private readonly CokerDbContext db;
		private readonly LoginUserData loginUserData;
		private readonly IHttpContextAccessor httpContextAccessor;
		private readonly IMapper mapper;
		public RemoteAppService(CokerDbContext db, LoginUserData loginUserData, IMapper mapper, IHttpContextAccessor httpContextAccessor) { 
			this.db = db;
			this.loginUserData = loginUserData;
			this.mapper = mapper;
			this.httpContextAccessor = httpContextAccessor;
		}
		public async Task<ResponseMessageDto> insertRemote(RemoteInputDto dto) {
			ResponseMessageDto response= new ResponseMessageDto();
			try {
				Core.Models.Remote r = mapper.Map<Core.Models.Remote>(dto);
                if (httpContextAccessor.HttpContext != null)
					r.BrowserInfo = httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();
				r.ClientIpAddress = loginUserData.GetClientIP();
				db.Add(r);
				await db.SaveChangesAsync();
				response.Success = true;
			}catch (Exception ex)
			{
				response.Message = ex.Message;
			}
			return response;
		}
		public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions) {
			long siteId =  await loginUserData.GetWebsiteId();
			var data = await (
                from d in db.Remotes
                join m in db.WebMenus on d.FK_WebmenuId equals m.Id
                where m.FK_WebsiteId == siteId
                group d by new
                {
                    d.FK_ProdId,
                    d.FK_WebmenuId,
                    d.FK_ArticleId,
                    d.FK_TechCertId,
                    d.ExecutionTime.Date
                } into g
                select new
                {
                    g.Key.FK_ProdId,
                    g.Key.FK_WebmenuId,
                    g.Key.FK_ArticleId,
                    g.Key.FK_TechCertId,
                    g.Key.Date,
                    Count = g.Count()
                }
            ).ToListAsync();
            ;

            if (data != null)
			{
                IEnumerable<RemoteListOtputDto>? dataQuery = from d in data
								join a in db.Article.Where(e => !e.IsDeleted) on d.FK_ArticleId equals a.Id
								select new RemoteListOtputDto{ 
									date = d.Date,
									type = "文章",
									name = a.Title??"",
									count = d.Count
								};
                dataQuery = dataQuery.Concat(
                    from d in data
                    join a in db.Prods.Where(e => !e.IsDeleted) on d.FK_ProdId equals a.Id
                    select new RemoteListOtputDto
                    {
                        date = d.Date,
                        type = "商品",
                        name = a.Title ?? "",
                        count = d.Count
                    }
				);
                dataQuery = dataQuery.Concat(
                    from d in data
                    join a in db.WebMenus.Where(e => !e.IsDeleted) on d.FK_WebmenuId equals a.Id
					where d.FK_ProdId == null && d.FK_ArticleId == null && d.FK_TechCertId == null
                    select new RemoteListOtputDto
                    {
                        date = d.Date,
                        type = "選單",
                        name = a.Title ?? "",
                        count = d.Count
                    }
                );
                dataQuery = dataQuery.Concat(
                    from d in data
                    join a in db.TechnicalCertificates.Where(e => !e.IsDeleted) on d.FK_TechCertId equals a.Id
                    select new RemoteListOtputDto
                    {
                        date = d.Date,
                        type = "標章認證",
                        name = a.Title ?? "",
                        count = d.Count
                    }
                );
                if (loadOptions.Sort == null)
                {
                    var Sort = new List<SortingInfo>{
                        new SortingInfo
                        {
                            Selector = "date",
                            Desc = true
                        },new SortingInfo
                        {
                            Selector = "count",
                            Desc = true
                        } 
                    };
                    loadOptions.Sort = Sort.ToArray();
                }
                var output = DataSourceLoader.Load(dataQuery, loadOptions);
				return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
			}
			else throw new Exception("查無資料");
		}
        //從資料庫撈使用者紀錄
		public async Task<JsonResult> GetPageList(DataSourceLoadOptions loadOptions) {
			long siteId = await loginUserData.GetWebsiteId();
			var data =
                from d in db.Remotes //使用者瀏覽紀錄
                join m in db.WebMenus.Where(e => e.FK_WebsiteId == siteId && !e.IsDeleted) on d.FK_WebmenuId equals m.Id
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
									date = d.Key.Date,
									count = d.Where(e => e.Date == d.Key.Date).Sum(e => e.count),
									MemCount = d.Count(),
								};
                if (loadOptions.Sort == null)
                {
                    var Sort = new List<SortingInfo>{new SortingInfo
                    {
                        Selector = "date",
                        Desc = true
                    } };
                    loadOptions.Sort = Sort.ToArray();
                }
                var output = DataSourceLoader.Load(dataQuery, loadOptions);
                //取日期跟時間
				return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
			}
			else throw new Exception("查無資料");
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
                response.Object = new GetRemoteCountOutputDto { 
                    remoteListOtputDtos = await dataQuery.ToListAsync()
                };
                response.Success = true;
                //取日期跟時間
                return response;
            }
            else throw new Exception("查無資料");
        }
    };
}

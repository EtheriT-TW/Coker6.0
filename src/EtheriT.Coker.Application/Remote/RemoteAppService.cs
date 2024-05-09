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
				if(httpContextAccessor.HttpContext != null)
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
			long siteId = await loginUserData.GetWebsiteId();
			var Query = await db.Remotes.Where(e => e.FK_WebsiteId == siteId).ToListAsync();
            var data = from e in Query
					   group e by new {
                           e.FK_ProdId,
                           e.FK_WebmenuId,
                           e.FK_ArticleId,
						   e.FK_TechCertId,
                           e.ExecutionTime.Date
                       } into g
					   select new {
                           g.Key.FK_ProdId,
                           g.Key.FK_WebmenuId,
                           g.Key.FK_ArticleId,
						   g.Key.FK_TechCertId,
                           g.Key.Date,
                           Count = g.Count(),
                           MemCount = g.GroupBy(x => x.ClientIpAddress).Count()
                       };

            if (data != null)
			{
                IEnumerable<RemoteListOtputDto>? dataQuery = from d in data
								join a in db.Article.Where(e => !e.IsDeleted) on d.FK_ArticleId equals a.Id
								select new RemoteListOtputDto{ 
									date = d.Date,
									type = "文章",
									name = a.Title??"",
									count = d.Count,
									MemCount = d.MemCount,
								};
                dataQuery = dataQuery.Concat(
                    from d in data
                    join a in db.Prods.Where(e => !e.IsDeleted) on d.FK_ProdId equals a.Id
                    select new RemoteListOtputDto
                    {
                        date = d.Date,
                        type = "商品",
                        name = a.Title ?? "",
                        count = d.Count,
                        MemCount = d.MemCount,
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
                        count = d.Count,
                        MemCount = d.MemCount,
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
                        count = d.Count,
                        MemCount = d.MemCount,
                    }
                );
                dataQuery.OrderBy(e => e.date);
                var output = DataSourceLoader.Load(dataQuery, loadOptions);
				return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
			}
			else throw new Exception("查無資料");
		}
		public async Task<JsonResult> GetPageList(DataSourceLoadOptions loadOptions) {
			long siteId = await loginUserData.GetWebsiteId();
			var data = await db.Remotes
                .Where(e => e.FK_WebsiteId == siteId).ToListAsync();
			if (data != null)
			{
				var dataQuery = from d in data
								group d by new
								{
									d.ExecutionTime.Date,
									d.FK_WebsiteId,
								} into d
								select new RemoteListOtputDto
								{
									date = d.Key.Date,
									count = d.Count(),
									MemCount = d.GroupBy(e => e.ClientIpAddress).Count(),
								};
				var output = DataSourceLoader.Load(dataQuery, loadOptions);
				return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
			}
			else throw new Exception("查無資料");
		}
	}
}

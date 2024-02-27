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
			var data = await db.Remotes
				.Include(e => e.Article)
				.Include(e => e.WebMenu)
				.Include(e => e.Prod)
				.Where(e => e.WebMenu.FK_WebsiteId == siteId)
				.ToListAsync();
			if (data != null)
			{
				var dataQuery = from d in data
								group d by new {
									d.ExecutionTime.Date,
									d.WebMenu.FK_WebsiteId,
									d.FK_WebmenuId,
									d.FK_ProdId,
									d.FK_ArticleId
								} into g
								select new RemoteListOtputDto{ 
									date = g.Key.Date,
									type = g.Select(e => e.Article != null ? "文章" : (e.Prod != null ? "商品" : "選單")).First() ?? "",
									name = g.Select(e => e.Article != null ? e.Article.Title : (e.Prod != null ? e.Prod.Title : e.WebMenu.Title)).First()??"",
									count = g.Count(),
									MemCount = g.GroupBy(e => e.ClientIpAddress).Count(),
								};
				var output = DataSourceLoader.Load(dataQuery, loadOptions);
				return new JsonResult(output, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
			}
			else throw new Exception("查無資料");
		}
		public async Task<JsonResult> GetPageList(DataSourceLoadOptions loadOptions) {
			long siteId = await loginUserData.GetWebsiteId();
			var data = await db.Remotes
				.Include(e => e.Article)
				.Include(e => e.WebMenu)
				.Include(e => e.Prod)
				.Where(e => e.WebMenu.FK_WebsiteId == siteId)
				.ToListAsync();
			if (data != null)
			{
				var dataQuery = from d in data
								group d by new
								{
									d.ExecutionTime.Date,
									d.WebMenu.FK_WebsiteId,
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

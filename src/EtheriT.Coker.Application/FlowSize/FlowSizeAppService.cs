using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.FlowSize;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.FlowSize
{
	public class FlowSizeAppService : IFlowSizeAppService
	{
		private readonly CokerDbContext _db;
		private readonly LoginUserData loginUserData;

		public FlowSizeAppService(CokerDbContext db, LoginUserData loginUserData)
		{
			this._db = db;
			this.loginUserData = loginUserData;
		}

		// 實現接口中的方法，用來查詢所有的 FlowSize 數據並轉換為 DTO
		public async Task<FlowSizeDto> GetMonthFlowSizes()
		{
			long userWebsiteId = await loginUserData.GetWebsiteId();
			var result = _db.FlowSizes
				.Where(flow => flow.FK_WebsiteId == userWebsiteId)
				.Join(_db.Websites,
					  flow => flow.FK_WebsiteId,
					  website => website.Id,
					  (flow, website) => new FlowSizeDto
					  {
						  WebsiteId = flow.FK_WebsiteId,
						  WebsiteName = website.OrgName,
						  RequestSize = flow.RequestSize,
						  ResponseSize = flow.ResponseSize,
						  Total = flow.Total,
						  ActionTime = flow.actionTime
					  })
				.GroupBy(f => f.WebsiteId)
				.Select(g => new FlowSizeDto
				{
					WebsiteId = g.Key, // 获取分组的WebsiteId
					WebsiteName = g.First().WebsiteName, // 获取分组中的WebsiteName
					RequestSize = g.Sum(f => f.RequestSize), // 总请求大小
					ResponseSize = g.Sum(f => f.ResponseSize), // 总响应大小
					Total = g.Sum(f => f.Total), // 总流量
					ActionTime = g.First().ActionTime // 获取最后的ActionTime
				})
				.FirstOrDefault()?? new FlowSizeDto();
			return result;
		}

		public async Task<JsonResult> GetFlowSizesList(DataSourceLoadOptions loadOptions)
		{
			long siteId = await loginUserData.GetWebsiteId();
			var data = _db.FlowSizes
				 .Where(e => e.FK_WebsiteId == siteId)
				 .GroupBy(f => f.actionTime.Date) // 按日期分組
				 .Select(g => new
				 {
					 Date = g.Key, // 分組鍵 (日期)
					 TotalRequest = g.Sum(x => x.RequestSize),  // 計算 RequestSize 的總和
					 TotalResponse = g.Sum(x => x.ResponseSize), // 計算 ResponseSize 的總和
					 Total = g.Sum(x => x.Total)  // 計算流量的總和
				 });


			if (data != null)
			{
				var resultData = await data.ToListAsync();

				var firstRecordDate = data.Select(d => d.Date).FirstOrDefault();
				var dataQuery = resultData.Select(d=>new 
								{
									ActionTime = d.Date,
					RequestSize = FormatBytes(d.TotalRequest),
					ResponseSize = FormatBytes(d.TotalResponse),
					Total = FormatBytes(d.Total)
				});
				if (loadOptions.Sort == null)
				{
					var Sort = new List<SortingInfo>{new SortingInfo
					{
						Selector = "ActionTime",
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

		public static string FormatBytes(long bytes)
		{
			if (bytes >= 1 << 30)
				return $"{(double)bytes / (1 << 30):0.##} GB";  // 保留兩位小數
			if (bytes >= 1 << 20)
				return $"{(double)bytes / (1 << 20):0.##} MB";  // 保留兩位小數
			if (bytes >= 1 << 10)
				return $"{(double)bytes / (1 << 10):0.##} KB";  // 保留兩位小數
			return $"{bytes} bytes";
		}
	}
}

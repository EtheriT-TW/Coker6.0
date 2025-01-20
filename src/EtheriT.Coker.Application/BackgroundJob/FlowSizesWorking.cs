using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EtheriT.Coker.Application.BackgroundJob
{

	public class FlowSizesWorking
	{
		private readonly CokerDbContext db;
		private readonly ITokenAppService tokenAppService;

		public FlowSizesWorking(CokerDbContext db, ITokenAppService tokenAppService)
		{
			this.db = db;
			this.tokenAppService = tokenAppService;
		}
		public void FlowSizeCollection()
		{
			var toDay = DateTime.Now.Date;
			//獲取站台名
			var siteId = db.Websites.Select(u => u.Id).ToList();
			List<EtheriT.Coker.Core.Models.FlowSize> flowSizes = new List<EtheriT.Coker.Core.Models.FlowSize>();

			foreach (var id in siteId)
			{
				var orgName = db.Websites.Where(u => u.Id == id).Select(u => u.OrgName).FirstOrDefault();
				var finalActionTime = db.FlowSizes.Where(f => f.FK_WebsiteId == id).OrderByDescending(f => f.actionTime).Select(f => f.actionTime).FirstOrDefault();
				if (finalActionTime>DateTime.MinValue && finalActionTime != null)
				{
					while (finalActionTime.Date < toDay.AddDays(-1)) {
						finalActionTime = finalActionTime.AddDays(1);
						addFlowSize(flowSizes, id, orgName, finalActionTime);
					}
				} else
				{
					addFlowSize(flowSizes, id, orgName, toDay.AddDays(-1));
				}
			}
			if (flowSizes.Any())
			{
				db.FlowSizes.AddRange(flowSizes);
				db.SaveChanges();
			}
		}

		private void addFlowSize(List<EtheriT.Coker.Core.Models.FlowSize> flowSizes, long id, string orgName, DateTime actionTime)
		{
			var logFilePath = "D:\\ET\\upload\\" + orgName + "\\logs\\" + actionTime.ToString("yyyy-MM-dd") + ".txt";
			var totalResponse = 0;
			var totalRequest = 0;
			var total = 0;
			if (!File.Exists(logFilePath))
			{
				return;
			}

			// 逐行讀取日誌檔案
			foreach (var line in File.ReadAllLines(logFilePath).Reverse())
			{
				if (total > 0 && totalRequest > 0 && totalResponse > 0)
				{
					break;
				}
				// 查找包含 "Response Size" 的行
				if (line.Contains("Total Response Size"))
				{
					totalResponse += int.Parse(Regex.Match(line, @"Total Response Size:\s*(\d+)\s*bytes").Groups[1].Value);  // 更新最後找到的符合條件的行
				}
				if (line.Contains("Total Request Size"))
				{
					totalRequest += int.Parse(Regex.Match(line, @"Total Request Size:\s*(\d+)\s*bytes").Groups[1].Value);
				}
				if (line.Contains("Total"))
				{
					total += int.Parse(Regex.Match(line, @"Total:\s*(\d+)\s*bytes").Groups[1].Value);
				}
			}
			flowSizes.Add(new EtheriT.Coker.Core.Models.FlowSize
			{
				FK_WebsiteId = id,
				RequestSize = totalRequest,
				ResponseSize = totalResponse,
				Total = total,
				actionTime = actionTime
			});
		}
	}
}

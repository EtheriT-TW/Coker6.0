using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using MiniExcelLibs;

namespace EtheriT.Coker.Application.BackgroundJob
{

	public class FlowSizesWorking
	{
		private readonly CokerDbContext db;
		private readonly ITokenAppService tokenAppService;
		private readonly Microsoft.Extensions.Configuration.IConfiguration Configuration;
		public FlowSizesWorking(CokerDbContext db, ITokenAppService tokenAppService, Microsoft.Extensions.Configuration.IConfiguration Configuration)
		{
			this.db = db;
			this.tokenAppService = tokenAppService;
			this.Configuration = Configuration;
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
				var logFilePath = $"{Configuration.GetValue<string>("VirtualDirectory:upload")}\\" + orgName + "\\logs";
				var dbDates = db.FlowSizes
					.Where(f=> f.FK_WebsiteId == id)
					.Select(f => f.actionTime.Date)  // 提取日期部分
					.ToList();  // 將結果加載到內存
				foreach (var fileDate in GetMatchingFiles(logFilePath))
				{
					if(fileDate == toDay)
					{
						break;
					}
					if (!dbDates.Contains(fileDate))
					{
						addFlowSize(flowSizes, id, orgName, logFilePath, fileDate);
					}
				}
				//DeleteOldLogs(logFilePath, TimeSpan.FromDays(3)); //刪除三天前的所有紀錄
			}
			if (flowSizes.Any())
			{
				db.FlowSizes.AddRange(flowSizes);
				db.SaveChanges();
			}
		}

		private void addFlowSize(List<EtheriT.Coker.Core.Models.FlowSize> flowSizes, long id, string orgName, string logsPath, DateTime actionTime)
		{
			var logFilePath = $"{logsPath}\\" + actionTime.ToString("yyyy-MM-dd") + ".txt";
			long totalResponse = 0;
			long totalRequest = 0;
			long total = 0;
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
					string str = Regex.Match(line, @"Total Response Size:\s*(\d+)\s*bytes").Groups[1].Value;
					if(str == "")
					{
						totalResponse += GetAllSize("Response Size",logFilePath);
					} else
					{
						totalResponse += long.Parse(Regex.Match(line, @"Total Response Size:\s*(\d+)\s*bytes").Groups[1].Value);  // 更新最後找到的符合條件的行
					}
				}
				if (line.Contains("Total Request Size"))
				{
					string str = Regex.Match(line, @"Total Request Size:\s*(\d+)\s*bytes").Groups[1].Value;
					if (str == "")
					{
						totalRequest += GetAllSize("Request Size", logFilePath);
					}
					else
					{
						totalRequest += long.Parse(Regex.Match(line, @"Total Request Size:\s*(\d+)\s*bytes").Groups[1].Value);  // 更新最後找到的符合條件的行
					}
				}
				if (line.Contains("Total"))
				{
					if (totalRequest > 0 && totalResponse > 0)
					{
						total += totalRequest + totalResponse;
					} else
					{
						total += long.Parse(Regex.Match(line, @"Total:\s*(\d+)\s*bytes").Groups[1].Value);
					}
				}
			}
			if (!db.FlowSizes.Any(f => f.FK_WebsiteId == id && f.actionTime.Date == actionTime.Date)) // 检查是否存在相同的 actionTime 日期
			{
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
		private long GetAllSize(string type, string logFilePath)
		{
			long total = 0;
			foreach (var line in File.ReadLines(logFilePath))
			{
				// 嘗試匹配每行中的 Response Size
				var match = Regex.Match(line, type+@":\s*(\d+)\s*bytes");
				if (match.Success)
				{
					// 提取數字部分，並轉換為 long 類型
					if (long.TryParse(match.Groups[1].Value, out long responseSize))
					{
						total += responseSize;
					}
				}
			}
			return total;
		}
		public List<DateTime> GetMatchingFiles(string folderPath)
		{
			List<DateTime> matchingDates = new List<DateTime>();
			var datePattern = @"\d{4}-\d{2}-\d{2}\.txt";  // 匹配 yyyy-MM-dd.txt 格式

			try
			{
				// 獲取所有檔案
				var files = System.IO.Directory.GetFiles(folderPath);
				// 使用正則表達式來篩選符合格式的檔案
				Regex regex = new Regex(datePattern);
				foreach (var file in files)
				{
					string fileName = Path.GetFileName(file);  // 取得檔案名稱
					if (regex.IsMatch(fileName))
					{
						// 從檔案名稱中解析出日期
						var dateStr = fileName.Substring(0, 10);  // yyyy-MM-dd
						if (DateTime.TryParse(dateStr, out DateTime fileDate))
						{
							matchingDates.Add(fileDate);  // 添加符合條件的日期
						}
					}
				}
			}
			catch (Exception ex)
			{
			}
			return matchingDates;
		}

		private void DeleteOldLogs(string folderPath, TimeSpan timeSpan)
		{
			try
			{
				// 定義日期格式的正則表達式，匹配 yyyy-MM-dd 格式的檔名
				var datePattern = @"(\d{4})-(\d{2})-(\d{2})\.txt";  // 匹配 yyyy-MM-dd.txt

				// 計算要刪除的時間點（現在時間減去3天）
				DateTime thresholdDate = DateTime.Now.Subtract(timeSpan);

				// 遍歷資料夾中的所有檔案
				var files = System.IO.Directory.GetFiles(folderPath);

				foreach (var file in files)
				{
					// 取得檔案名稱
					string fileName = Path.GetFileName(file);

					// 使用正則表達式來匹配檔名中的日期部分
					var match = Regex.Match(fileName, datePattern);
					if (match.Success)
					{
						// 提取檔案中的日期，並嘗試將其轉換為 DateTime
						string dateStr = $"{match.Groups[1].Value}-{match.Groups[2].Value}-{match.Groups[3].Value}";
						if (DateTime.TryParse(dateStr, out DateTime fileDate))
						{
							// 比較日期，刪除三天前的檔案
							if (fileDate < thresholdDate)
							{
								Console.WriteLine($"刪除檔案: {fileName}, 檔案日期: {fileDate:yyyy-MM-dd}");
								File.Delete(file);  // 刪除檔案
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"發生錯誤: {ex.Message}");
			}
		}
	}
}

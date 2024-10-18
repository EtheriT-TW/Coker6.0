using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.FileManagement;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Shared.Dto.Remote;
using EtheriT.Coker.Application.Shared.Remote;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.EntityFrameworkCore.Migrations;
using EtheriT.Coker.Web.MVC.Models.Dacshboard;
using EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Globalization;
using System.IO.Pipelines;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class DashboardController : Controller
    {
        private readonly LoginUserData loginUserData;//獲取後台登入後選擇編輯哪個站點
		private readonly IRemoteAppService remoteAppService;
        private readonly IConfiguration configuration;
        private List<long> remoteList = new List<long>();
        private List<DateTime> dateList = new List<DateTime>();
		public DashboardController(LoginUserData loginUserData, IRemoteAppService remoteAppService, IConfiguration configuration) { 
            this.loginUserData = loginUserData;
            this.remoteAppService = remoteAppService;
            this.configuration = configuration;
        }
        //非同步 用Task的模式讀取
        public async Task<IActionResult> Index()
        {			
			string orgName = await loginUserData.GetWebsiteOrgName();//獲取後台登入後選擇編輯哪個站點
			long orgId = loginUserData.GetFrontWebsiteId();//獲取站台Id
            string filePath = $"{configuration.GetValue<string>("VirtualDirectory:upload")}\\{orgName}";
            var result = await remoteAppService.GetRemoteCount(new GetRemoteCountInputDto { 
                StartDate = DateTime.Now.AddDays(-7),
                EndDate = DateTime.Now
            });
            var remoteItem = new List<long>();
            var remoteMemCount = new List<long>();
            var dateItem = new List<string>();
			long totalRemoteCount = 0;
			long totalMemCount = 0;
			var today = DateTime.Today;

			var obj = await remoteAppService.GetPageList(new DevExtreme.AspNet.Mvc.DataSourceLoadOptions());
			var loadResult = (DevExtreme.AspNet.Data.ResponseModel.LoadResult)obj.Value;
			var getTotle = loadResult.data.Cast<RemoteListOtputDto>().ToList();
			DateTime? earliestDate = getTotle.OrderBy(e => e.date).FirstOrDefault().date;
			DateTime firstTime = (DateTime)earliestDate;

			if (result.Success) {
                var items = ((GetRemoteCountOutputDto)result.Object).remoteListOtputDtos;
				if (earliestDate != null)
				{
					// 从最早日期开始到今天
					for (DateTime d = earliestDate.Value.Date; d <= DateTime.Today; d = d.AddDays(1))
					{
						RemoteListOtputDto? item = getTotle.Find(e => e.date.Date == d.Date);
						if (item == null)
						{
							// 没有数据则累加0
							totalRemoteCount += 0;
							totalMemCount += 0;
						}
						else
						{
							totalRemoteCount += item.count;
							totalMemCount += item.MemCount;
						}
					}
				}

				for (int i = 0; i < 7; i++)
                {
                    DateTime d = DateTime.Now.Date.AddDays(-i);
                    RemoteListOtputDto? item = items.Find(e => e.date.Day == d.Day);
                    if (item == null)
                    {                        
                        remoteItem.Add(0);
                        remoteMemCount.Add(0);
                    }
                    else
                    {                       
                        remoteItem.Add(item.count);
                        remoteMemCount.Add(item.MemCount);
                    }
                    dateItem.Add(d.ToString("MM/dd"));
                }
                dateItem.Reverse();
                remoteMemCount.Reverse();
                remoteItem.Reverse();
			}


            DashboardModel model = new DashboardModel
            {
                //客戶訂單
                Orders = new List<OrderItem> {
                    new OrderItem{
                        Id="000000317",
                        Name="黃○瑜",
                        Time=DateTime.Now.AddHours(-12),
                        Price=540.0,
                        Statues="審核中"
                    },new OrderItem{
                        Id="000000318",
                        Name="張○君",
                        Time=DateTime.Now.AddHours(-6),
                        Price=900.0,
                        Statues="審核中"
                    },new OrderItem{
                        Id="000000319",
                        Name="顏○禎",
                        Time=DateTime.Now.AddHours(-4),
                        Price=900.0,
                        Statues="審核中"
                    },new OrderItem{
                        Id="000000320",
                        Name="張○偉",
                        Time=DateTime.Now.AddHours(-2),
                        Price=420.0,
                        Statues="審核中"
                    }
                },
                //使用空間
                CalcuateDirectorySize = CalculateDirectorySize(filePath),
                LastChangDate = LastChangDate(filePath),
                Remote = new WebsitesRemote
                {
                    WebsitesRemotesCount = remoteItem, //全站導覽人次
                    WebsitesRemotesDate = dateItem, //最近7天的時間
                    WebsitesRemotesMemCount = remoteMemCount,
                    SumCount = remoteItem.Sum(),
                    SumMemCount = remoteMemCount.Sum(),
                    LastUpdateDate = today.ToString("MM-01") + " 至 " + today.ToString("MM-dd"),
                    TotleCount = totalRemoteCount,
                    TotleMemCount = totalMemCount,
                    FirstTime = firstTime
                }
            };
            return View(model);
        }

        // 獲取資料夾的最後修改時間
        public static string LastChangDate(string DirRoute)
        {
			try
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(DirRoute);
				DateTime lastModified = directoryInfo.LastWriteTime;
                return lastModified.ToString();
			}
			catch (Exception ex)
			{
				Console.WriteLine("錯誤: " + ex.Message);
                return "發生錯誤";
			}
		}

		//計算某個資料夾的大小 參數=資料夾路徑
		public static string CalculateDirectorySize(string DirRoute)
		{
			try
			{
				var directoryInfo = new DirectoryInfo(DirRoute);
                //如果資料夾存在
                if (directoryInfo.Exists)
                {
                    long totalSizeInBytes = directoryInfo
                    .EnumerateFiles("*", SearchOption.AllDirectories)
                    .Sum(file => file.Length);

					// 單位換算
					return FormatSize(totalSizeInBytes);
				}
                return "請確認資料路徑是否正確";
			}
			catch
			{
				return "發生錯誤";
			}
		}

		//給CalculateDirectorySize換算目前容量最適合使用得單位
		private static string FormatSize(long bytes)
		{
			string[] sizes = { "B", "KB", "MB", "GB", "TB" };
			double len = bytes;
			int order = 0;
			while (len >= 1024 && order < sizes.Length - 1)
			{
				order++;
				len = len / 1024;
			}
			return $"{len:0.##} {sizes[order]}";
		}
	}
}

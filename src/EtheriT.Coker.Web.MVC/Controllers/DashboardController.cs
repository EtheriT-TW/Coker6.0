using DevExtreme.AspNet.Mvc.FileManagement;
using EtheriT.Coker.Application;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.EntityFrameworkCore.Migrations;
using EtheriT.Coker.Web.MVC.Models.Dacshboard;
using EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class DashboardController : Controller
    {

        private readonly LoginUserData loginUserData;//獲取後台登入後選擇編輯哪個站點
		private readonly CokerDbContext db;//資料庫連接
		public DashboardController(LoginUserData loginUserData, CokerDbContext db) { 
            this.loginUserData = loginUserData;
            this.db = db;
        }
        //非同步 用Task的模式讀取
        public async Task<IActionResult> Index()
        {			
			string orgName = await loginUserData.GetWebsiteOrgName();//獲取後台登入後選擇編輯哪個站點
			long orgId = loginUserData.GetFrontWebsiteId();//獲取站台Id
            string filePath = $"D:\\ET\\upload\\{orgName}";
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

                //全站導覽人次
                WebsitesRemotes = WebsitesRemotes(orgId, db)
            };
            return View(model);
        }
		
		//站點的瀏覽人次
		public static List<long> WebsitesRemotes(long websiteId, CokerDbContext db)
        {
			List<long> RemoteCount = new List<long>();
            DateTime time = new DateTime();
			try
            {               
                for (int i = 0; i < 7; i++)
                {
                    long data = db.Remotes //使用者瀏覽紀錄
                        .Where(e => e.FK_WebsiteId == websiteId && e.ExecutionTime.Date == time.Date.AddDays(-i))
                        .Count();
					RemoteCount.Add(data);
					Console.WriteLine("///////////////" + time + "//////////////////////");
				}
                foreach (int i in RemoteCount)
                {
					Console.WriteLine("///////////////" + i + "//////////////////////");
				}				
				return RemoteCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\\\\\\\\\\\\\\\\\\\\\\\\"+ex.Message+"////////////////////////////");
                return RemoteCount;
            }
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

					// 將總大小從位元組轉換為 GB
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

using DevExtreme.AspNet.Mvc.FileManagement;
using EtheriT.Coker.Application;
using EtheriT.Coker.Web.MVC.Models.Dacshboard;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class DashboardController : Controller
    {
        //獲取後台登入後選擇編輯哪個站點
        private readonly LoginUserData loginUserData;

		public DashboardController(LoginUserData loginUserData) { 
            this.loginUserData = loginUserData;
        }
        //非同步 用Task的模式讀取
        public async Task<IActionResult> Index()
        {
			//獲取後台登入後選擇編輯哪個站點
			string orgName = await loginUserData.GetWebsiteOrgName();

			DashboardModel model = new DashboardModel
            {
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

                CalcuateDirectorySize = CalculateDirectorySize($"D:\\ET\\upload\\{orgName}")
            };
            return View(model);
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

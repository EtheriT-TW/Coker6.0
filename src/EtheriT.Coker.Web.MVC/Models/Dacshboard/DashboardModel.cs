namespace EtheriT.Coker.Web.MVC.Models.Dacshboard
{
    public class DashboardModel
    {

		public List<long> WebsitesRemotes { get; set; }//瀏覽人次
		public string LoadDate {  get; set; }//瀏覽人次的統計日期
		public string CalcuateDirectorySize { get; set; }//使用空間
		public string LastChangDate { get; set; }//最後修改時間
		public List<OrderItem> Orders { get; set; }//客戶訂單
    }
}

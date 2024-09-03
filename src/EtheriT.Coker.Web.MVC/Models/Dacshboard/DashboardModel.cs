namespace EtheriT.Coker.Web.MVC.Models.Dacshboard
{
    public class DashboardModel
    {

		public WebsitesRemote Remote { get; set; } //瀏覽人次
        public string CalcuateDirectorySize { get; set; }//使用空間
		public string LastChangDate { get; set; }//最後修改時間
		public List<OrderItem> Orders { get; set; }//客戶訂單
    }
}

namespace EtheriT.Coker.Web.MVC.Models.Dacshboard
{
    public class DashboardModel
    {

		public WebsitesRemote Remote { get; set; } //瀏覽人次
        public string CalcuateDirectorySize { get; set; }//使用空間
		public string LastChangDate { get; set; }//使用空間最新修改時間
        public WebsitesFlowSize FlowSize { get; set; } //使用流量
        public string StatisticalTime { get; set; } //流量統計週期
        public string MonthTotalFlowSize { get; set; } //當月總使用流量
        public string ToMonth { get; set; } //當月月份
		public List<OrderItem> Orders { get; set; }//客戶訂單
    }
}

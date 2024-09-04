namespace EtheriT.Coker.Web.MVC.Models.Dacshboard
{
    public class WebsitesRemote
    {
        public List<long> WebsitesRemotesCount { get; set; }//瀏覽人次
        public List<long> WebsitesRemotesMemCount { get; set; }//瀏覽人數       
        public long SumCount { get; set; }//當月瀏覽人次
        public long SumMemCount { get; set; }//當月總瀏覽人數
        public List<string> WebsitesRemotesDate { get; set; } //這個禮拜
        public string LastUpdateDate { get; set; }//統計區間時間段
    }
}

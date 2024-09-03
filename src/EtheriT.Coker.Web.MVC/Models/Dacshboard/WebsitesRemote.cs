namespace EtheriT.Coker.Web.MVC.Models.Dacshboard
{
    public class WebsitesRemote
    {
        public List<long> WebsitesRemotesCount { get; set; }//瀏覽人次
        public List<long> WebsitesRemotesMemCount { get; set; }//瀏覽人數
        public List<string> WebsitesRemotesDate { get; set; } //這個禮拜
        public string LoadDate { get; set; }//最後更新時間
    }
}

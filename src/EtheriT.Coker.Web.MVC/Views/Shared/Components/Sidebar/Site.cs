namespace EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar
{
    public class Site
    {
        public string WebRootLink { get; set; }
        public string OrgName { get; set; }
		public string Title { get; set; }
        public Dictionary<string, string> PageTitleMap { get; set; } = new();
        public List<JobMenu> Jobs { get; set; }
    }
}

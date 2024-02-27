namespace EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar
{
    public class JobMenu
    {
        public string? Icon { get; set; }
        public string Title { get; set; }
        public string PageName { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string? CollapseId { get; set; }
        public bool Enable { get; set; } = true;
        public bool CanCreate { get; set; }
        public bool CanRemove { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanVisble { get; set; }
        public List<JobMenu>? jobItemModels { get; set; }
    }
}

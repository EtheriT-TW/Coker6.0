namespace EtheriT.Coker.Web.MVC.Startup
{
    public sealed class BonusPermission
    {
        public static readonly BonusPermission DenyAll = new BonusPermission();
        public bool CanExe { get; set; } = false;
        public bool CanEdit { get; set; } = false;
    }
}

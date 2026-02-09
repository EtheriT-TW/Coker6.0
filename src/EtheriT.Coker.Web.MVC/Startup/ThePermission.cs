namespace EtheriT.Coker.Web.MVC.Startup
{
	public class ThePermission
	{
        public static readonly ThePermission DenyAll = new ThePermission();
        public bool Initable { get; set; }
		public bool systemManager { get; set; }
        public bool superManager { get; set; }
        public bool CanVisble { get; set; }
		public bool CanUpdate { get; set;}
		public bool CanRemove { get; set;}
		public bool CanCreate { get; set;}
	}
}

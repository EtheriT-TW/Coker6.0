namespace EtheriT.Coker.Web.MVC.Startup
{
	public static class ThePermission
	{
        public static bool Initable { get; set; }
		public static bool systemManager { get; set; }
        public static bool superManager { get; set; }
        public static bool CanVisble { get; set; }
		public static bool CanUpdate { get; set;}
		public static bool CanRemove { get; set;}
		public static bool CanCreate { get; set;}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.ConsoleApp.Models.OldDB
{
	[Table("tag")]
	public class tag
	{
		public int id { get; set; }
		public string title { get; set; }
		public string? link { get; set; }
		public string? cdate { get; set; }
		public string? edate { get; set; }
		public int? tg_id { get; set; }
	}
}

using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
	public class StoreSetGroup : FullAuditedEntity
	{
		[StringLength(100)]
		public string Title { get; set; }
		public string? Description { get; set; }
		[StringLength(150)]
		public string Image { get; set; }
		public List<StoreSet> StoreSets { get; set; }
	}
}

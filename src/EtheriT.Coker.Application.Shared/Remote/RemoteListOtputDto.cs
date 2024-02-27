using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Remote
{
	public class RemoteListOtputDto
	{
		public string type {  get; set; }
		public string name { get; set; }
		public long count { get; set; }
		public long MemCount { get; set; }
		public DateTime date { get; set; }
	}
}

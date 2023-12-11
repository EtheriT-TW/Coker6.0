using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Authorizaion
{
	public class UserSimplifyDto
	{
		public long Id { get; set; }
		public string Account { get; set; }
		public string UserName { get; set; }
		public string token { get; set; }
	}
}

using EtheriT.Coker.Application.Shared.Dto.enumType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Member
{
	public class ManagerAllListDto
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public string? TelPhone { get; set; }
		public string Email { get; set; }
		public string Roles { get; set; }
		public UserStatusEnum Status { get; set; }
	}
}

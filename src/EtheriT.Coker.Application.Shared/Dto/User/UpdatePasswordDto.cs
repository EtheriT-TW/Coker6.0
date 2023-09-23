using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.User
{
	public class UpdatePasswordDto
	{
		public string Password { get; set; }
		public string NewPassword { get; set; }
		public string NewPasswordChecked { get; set; }
	}
}

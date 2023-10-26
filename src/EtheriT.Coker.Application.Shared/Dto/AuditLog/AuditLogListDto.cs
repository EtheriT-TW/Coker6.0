using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Dto.AuditLog
{
	public class AuditLogListDto
	{
		public long Id { get; set; }
		public string ServiceName { get; set; }
		public string MethodName { get; set; }
		public string ClientIpAddress { get; set; }
		public DateTime ExecutionTime { get; set; }
		public string Parameters { get; set; }
		public string ReturnValue { get; set; }
		public string ClientName { get; set; }
	}
}

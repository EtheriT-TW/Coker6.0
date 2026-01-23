using EtheriT.Coker.Application.Shared.Dto.enumType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Order
{
	public class OrderUpdateStatusDto
	{
		public long Id { get; set; }
		public OrderStatusEnum Status { get; set; }
		public string? Memo {  get; set; } = string.Empty;
	}
}

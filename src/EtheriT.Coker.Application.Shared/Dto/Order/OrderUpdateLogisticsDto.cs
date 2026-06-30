using EtheriT.Coker.Application.Shared.Dto.enumType.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderUpdateLogisticsDto
    {
        public long Id { get; set; }
        public string? TrackingNumber { get; set; } = string.Empty;
    }
}

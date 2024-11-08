using EtheriT.Coker.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderDataGetAllDto: ResponseMessageDto
    {
        public List<OrderDataGetDto> OrderData { get; set; }
        public int? Page_Total { get; set; }
    }
}

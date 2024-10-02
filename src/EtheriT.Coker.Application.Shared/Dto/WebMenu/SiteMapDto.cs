using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Dto
{
    public class SiteMapDto: ResponseMessageDto
    {
        public List<MenuItemDto> Maps { get; set; } = new List<MenuItemDto>();
    }
}

using EtheriT.Coker.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.WebMenu
{
    public class GetMenuContenDto: ResponseMessageDto
    {
        public MenuSaveContenDto? Conten { get; set; }
    }
}

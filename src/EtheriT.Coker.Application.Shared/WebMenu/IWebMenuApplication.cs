using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Webs.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.WebMenu
{
    internal interface IWebMenuApplication
    {
        public Task<List<WebsDto>> GetAll();
        public Task<ResponseMessageDto> AddMenu();
        public Task<ResponseMessageDto> saveConten();
        public Task<ResponseMessageDto> importConten();
    }
}

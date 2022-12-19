using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Webs.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application
{
    public interface IWebMenuApplication
    {
        public Task<SiteMapDto> GetAll();
        public Task<ResponseMessageDto> CreateOrEdit(MenuItemDto dto);
        public Task<ResponseMessageDto> saveConten(MenuSaveContenDto dto);
        public Task<ResponseMessageDto> importConten(MenuContenDto dto);
    }
}

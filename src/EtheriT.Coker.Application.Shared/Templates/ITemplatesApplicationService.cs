using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Templates
{
    public interface ITemplatesApplicationService
    {
        public Task<TemplatesDto?> GetDefaultTemplatesAsync();
        public Task<ResponseMessageDto> GetDefaultFooterTemplatesAsync();
        public Task<ResponseMessageDto> importDefaultFooter(MenuSaveContenDto dto);
        public Task<ResponseMessageDto> saveDefaultFooter(MenuSaveContenDto dto);
    }
}

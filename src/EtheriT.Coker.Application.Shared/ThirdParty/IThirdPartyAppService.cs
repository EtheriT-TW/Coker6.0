using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.ThirdParty
{
    public interface IThirdPartyAppService
    {
        public Task<ResponseMessageDto> GetAllThirdParty();
        public Task<ResponseMessageDto> SaveThirdParty(ThirdPartySaveInputDto dto);
    }
}

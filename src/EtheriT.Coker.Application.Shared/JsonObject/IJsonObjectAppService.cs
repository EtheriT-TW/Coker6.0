using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.JsonObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.JsonObject
{
    public interface IJsonObjectAppService
    {
        public Task<ResponseMessageDto> AddUp(JsonObjectAddDto dto);
    }
}

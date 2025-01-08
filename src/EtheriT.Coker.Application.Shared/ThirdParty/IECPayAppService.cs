using EtheriT.Coker.Application.Dto;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.ThirdParty
{
    public interface IECPayAppService
    {
        public Task<ResponseMessageDto> ECPayHeaders(long ohid);
    }
}

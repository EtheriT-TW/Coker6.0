using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using Microsoft.AspNetCore.Mvc;
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
        public Task<JsonResult> GetDisplayPayment();
        public Task<List<ThirdPartyKeypairItemOutputDto>> GetPaymentResult(long paytypeid);
    }
}

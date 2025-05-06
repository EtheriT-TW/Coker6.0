using EtheriT.Coker.Application.Dto;

namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto
{
    public class ECPayCreditDetailReturnDto : ResponseMessageDto
    {
        public string TradeNo;
        public string Status;
        public int Amount;
        public string AuthTime;
    }
}

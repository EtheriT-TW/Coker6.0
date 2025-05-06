
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto
{
    public class ECPayCreditDetailResponseDataDto
    {
        public string RtnMsg { get; set; }
        public class RtnValueDto
        {
            public int TradeID { get; set; }
            public int Amount { get; set; }
            public int ClsAmt { get; set; }
            public string AuthTime { get; set; }
            public string Status { get; set; }
        }
        public RtnValueDto RtnValue {  get; set; }
        public class CloseDataDto
        {
            public string Status { get; set; }
            public int Amount { get; set; }
            public string DateTime { get; set; }
        }
        public CloseDataDto CloseData { get; set; }

    }
}

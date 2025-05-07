
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto
{
    public class ECPayCardInfoDto
    {
        public string Redeem { get; set; } = "0";
        public int PeriodAmount { get; set; }
        public string PeriodType { get; set; }
        public int Frequency { get; set; }
        public int ExecTimes { get; set; }
        public string OrderResultURL { get; set; }
        public string PeriodReturnURL { get; set; }
        public string CreditInstallment { get; set; } = "";
        public string FlexibleInstallment { get; set; }
    }
}


namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayDto
{
    public class ECPayResponseDataDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int RtnCode { get; set; }
        public string RtnMsg { get; set; }
        public string PlatformID { get; set; } 
        public string MerchantID { get; set; }
        public string? Token { get; set; }
        public string? TokenExpireDate { get; set; }
        public class OrderInfoDto
        {
            public string MerchantTradeNo { get; set; }
            public string? TradeNo { get; set; }
            public int? TradeAmt { get; set; }
            public string? TradeDate { get; set; }
            public string? PaymentType { get; set; }
            public string? PaymentDate { get; set; }
            public double? ChargeFee { get; set; }
            public double? ProcessFee { get; set; }
            public string? TradeStatus { get; set; }
        }
        public OrderInfoDto OrderInfo { get; set; }
        public class ThreeDInfoDto
        {
            public string? ThreeDURL { get; set; }
        }
        public ThreeDInfoDto ThreeDInfo { get; set; }
        public class UnionPayInfoDto
        {
            public string? UnionPayURL { get; set; }
        }
        public UnionPayInfoDto UnionPayInfo { get; set; }
        public class CardInfoDto
        {
            public string? AuthCode { get; set; }
            public int? Gwsr { get; set; }
            public string? ProcessDate { get; set; }
            public int? Amount { get; set; }
            public int? Stage { get; set; }
            public int? Stast { get; set; }
            public int? Staed { get; set; }
            public int? Eci { get; set; }
            public string? Card6No { get; set; }
            public string? Card4No { get; set; }
            public int? RedDan { get; set; }
            public int? RedDeAmt { get; set; }
            public int? RedOkAmt { get; set; }
            public int? RedYet { get; set; }
            public string? PeriodType { get; set; }
            public string? Frequency { get; set; }
            public int? ExecTimes { get; set; }
            public int? PeriodAmount { get; set; }
            public int? TotalSuccessTimes { get; set; }
            public int? TotalSuccessAmount { get; set; }
            public string? IssuingBank { get; set; }
            public string? IssuingBankCode { get; set; }
        }
        public CardInfoDto CardInfo { get; set; }
        public class ATMInfoDto
        {
            public string? ATMAccBank { get; set; }
            public string? ATMAccNo { get; set; }
            public string? BankCode { get; set; }
            public string? vAccount { get; set; }
            public string? ExpireDate { get; set; }
        }
        public ATMInfoDto ATMInfo { get; set; }
        public class CVSInfoDto
        {
            public string? PayFrom { get; set; }
            public string? PayStoreID { get; set; }
            public string? PayStoreName { get; set; }
            public string? PaymentNo { get; set; }
            public string? ExpireDate { get; set; }
            public string? PaymentURL { get; set; }
        }
        public CVSInfoDto CVSInfo { get; set; }
        public class BarcodeInfoDto
        {
            public string? PayFrom { get; set; }
            public string? ExpireDate { get; set; }
            public string? Barcode1 { get; set; }
            public string? Barcode2 { get; set; }
            public string? Barcode3 { get; set; }
        }
        public BarcodeInfoDto BarcodeInfo { get; set; }
        public string? CustomField { get; set; }
        public int? SimulatePaid { get; set; }
    }
}

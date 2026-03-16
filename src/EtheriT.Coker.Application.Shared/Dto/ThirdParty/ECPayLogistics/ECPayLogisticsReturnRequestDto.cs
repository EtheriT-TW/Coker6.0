
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.ECPayLogistics
{
    public class ECPayLogisticsReturnRequestDto
    {
        public string MerchantID { get; set; }
        public string AllPayLogisticsID { get; set; }
        public string ServerReplyURL { get; set; }
        public string GoodsName { get; set; }
        public int GoodsAmount { get; set; }
        public int CollectionAmount { get; set; }
        public string ServiceType { get; set; }
        public string SenderName { get; set; }
        public string SenderPhone { get; set; }
        public string Remark { get; set; }
        public string PlatformID { get; set; }
        public string CheckMacValue { get; set; }

    }
}

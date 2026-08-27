using EtheriT.Coker.Application.Shared.Dto.enumType;

using EtheriT.Coker.Application.Shared.Dto.enumType.Logistics;

namespace EtheriT.Coker.Application.Shared.Dto.Recipients
{
    public class RecipientsDto
    {
        public long Id { get; set; }
        public Guid UUID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string? ZipCode { get; set; }
        public string CellPhone { get; set; }
        public string TelePhone { get; set; }
        public SexEnum Sex { get; set; }
        public ShippingTypeEnum? LogisticsType { get; set; }
        public string? CVSStoreID { get; set; }
        public string? CVSStoreName { get; set; }
        public string? CVSAddress { get; set; }
        public string? CVSTelephone { get; set; }
        public string? CVSOutSide { get; set; }
        public long FK_WebsiteId { get; set; }
    }
}

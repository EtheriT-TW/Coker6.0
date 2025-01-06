
namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderHeaderGetAllListDto
    {
        public Guid UUID { get; set; }
        public string MemberId { get; set; }
        public string Id { get; set; }
        public string Orderer { get; set; }
        public string RecipientAddress { get; set; }
        public string Shipping { get; set; }
        public string Payment { get; set; }
        public string State { get; set; }
        public int Total { get; set; }
        public virtual DateTime CreationTime { get; set; }

    }
}

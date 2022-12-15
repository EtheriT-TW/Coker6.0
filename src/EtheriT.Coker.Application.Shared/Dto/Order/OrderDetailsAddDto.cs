
namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderDetailsAddDto
    {
        public long FK_OHId { get; set; }
        public List<int> FK_SCId_Arr { get; set; }
        public Guid FK_TId { get; set; }

    }
}

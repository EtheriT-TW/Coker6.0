
namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderDetailsAddDto
    {
        public long FK_OId { get; set; }
        public long FK_PSId { get; set; }
        public int Amount { get; set; }
        public double Subtotal { get; set; }
        public int? Bonus { get; set; }

    }
}

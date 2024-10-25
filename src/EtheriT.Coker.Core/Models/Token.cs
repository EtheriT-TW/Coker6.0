
namespace EtheriT.Coker.Core.Models
{
    public class Token
    {
        public Guid id { get; set; } //RefreshToken
        public Guid UUID { get; set; }
        public long? UserID { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }
        public string ip { get; set; }
        public long websiteId { get; set; }
        public ICollection<Prod_Log> Prod_Logs { get; set; }
        public ICollection<ShoppingCart> ShoppingCarts { get; set; }
    }
}

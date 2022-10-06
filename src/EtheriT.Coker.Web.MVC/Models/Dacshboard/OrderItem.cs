using System.Security.Cryptography.X509Certificates;

namespace EtheriT.Coker.Web.MVC.Models.Dacshboard
{
    public class OrderItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public double Price { get; set; }
        public string? Statues { get; set; }
    }
}

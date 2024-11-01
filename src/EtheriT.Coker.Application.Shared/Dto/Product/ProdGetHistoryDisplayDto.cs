
namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProdGetHistoryDisplayDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Introduction { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public string? Image { get; set; }
        public List<double> Price { get; set; }
        public string? ItemNo { get; set; }
    }
}

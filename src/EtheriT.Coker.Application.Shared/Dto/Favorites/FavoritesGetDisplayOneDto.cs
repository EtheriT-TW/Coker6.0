
using EtheriT.Coker.Application.Shared.Dto.Product;

namespace EtheriT.Coker.Application.Shared.Dto.Favorites
{
    public class FavoritesGetDisplayOneDto
    {
        public long FId { get; set; }
        public long PId { get; set; }
        public string Title { get; set; }
        public string Introduction { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public string? Image { get; set; }
        public string OriPrice { get; set; }
        public string Price { get; set; }
        public string? ItemNo { get; set; }
    }
}

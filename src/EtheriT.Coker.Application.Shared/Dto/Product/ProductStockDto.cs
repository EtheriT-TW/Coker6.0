
namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProductStockDto
    {
        public long Id { get; set; }
        public long Pid { get; set; }
        public long? FK_ST1id { get; set; }
        public long? FK_S1id { get; set; }
        public string? S1_Title { get; set; }
        public string? S1_Name { get; set; }
        public long? FK_ST2id { get; set; }
        public long? FK_S2id { get; set; }
        public string? S2_Title { get; set; }
        public string? S2_Name { get; set; }
        public double Price { get; set; }
        public int? Stock { get; set; }
        public int Ser_No { get; set; }
        public string SubItemNo { get; set; } = string.Empty;
        public int? Min_Qty { get; set; }
        public int? Alert_Qty { get; set; }
        public List<ProductPriceDto> Prices { get; set; }
    }
}

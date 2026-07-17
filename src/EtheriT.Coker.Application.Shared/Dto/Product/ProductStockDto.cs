
using EtheriT.Coker.Application.Shared.Dto.Files;

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
        public decimal Price { get; set; }
        public int? OrderStock { get; set; }
        public int? Stock { get; set; }
        public int PackingPoint { get; set; } = 1;
        public int? OldStock { get; set; }
        public int Ser_No { get; set; }
        public string SubItemNo { get; set; } = string.Empty;
        public string? SpecDescription { get; set; }
        public int? Min_Qty { get; set; }
        public int? Alert_Qty { get; set; }
        public bool TimePrice { get; set; }
        public decimal SuggestPrice { get; set; }
        public long TempPSid { get; set; }   // 前端暫存規格識別，存檔後回傳 id 對照用
        public List<FileGetProdDisplayDto> Multimedia { get; set; } = new();   // 規格圖（讀取時帶出）
        public List<ProductPriceDto> Prices { get; set; }
    }
}
